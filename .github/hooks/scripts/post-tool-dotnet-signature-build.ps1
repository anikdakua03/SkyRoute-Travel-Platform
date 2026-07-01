$ErrorActionPreference = 'Stop'

function Get-NormalizedRelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$WorkspaceRoot
    )

    $candidate = $Path.Trim().Trim('"')
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        return $null
    }

    if ($candidate.StartsWith('file://', [System.StringComparison]::OrdinalIgnoreCase)) {
        return $null
    }

    $fullPath = $candidate
    if (-not [System.IO.Path]::IsPathRooted($candidate)) {
        $fullPath = Join-Path $WorkspaceRoot $candidate
    }

    try {
        $resolved = [System.IO.Path]::GetFullPath($fullPath)
        $workspaceResolved = [System.IO.Path]::GetFullPath($WorkspaceRoot)

        if (-not $resolved.StartsWith($workspaceResolved, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $null
        }

        $relative = $resolved.Substring($workspaceResolved.Length).TrimStart('\\', '/')
        return $relative.Replace('\\', '/')
    }
    catch {
        return $null
    }
}

function Collect-ValuesByPropertyName {
    param(
        [Parameter(Mandatory = $true)]$InputObject,
        [Parameter(Mandatory = $true)][string[]]$PropertyNames
    )

    $results = [System.Collections.Generic.List[object]]::new()

    function Visit {
        param($Node)

        if ($null -eq $Node) {
            return
        }

        if ($Node -is [System.Collections.IDictionary]) {
            foreach ($key in $Node.Keys) {
                $value = $Node[$key]
                if ($PropertyNames -contains [string]$key) {
                    $results.Add($value)
                }
                Visit -Node $value
            }
            return
        }

        if ($Node -is [System.Collections.IEnumerable] -and -not ($Node -is [string])) {
            foreach ($item in $Node) {
                Visit -Node $item
            }
            return
        }

        if ($Node.PSObject -and $Node.PSObject.Properties) {
            foreach ($property in $Node.PSObject.Properties) {
                if ($PropertyNames -contains $property.Name) {
                    $results.Add($property.Value)
                }
                Visit -Node $property.Value
            }
        }
    }

    Visit -Node $InputObject
    return $results
}

function Try-ExtractMethodName {
    param(
        [Parameter(Mandatory = $true)][string]$SignatureLine
    )

    $match = [System.Text.RegularExpressions.Regex]::Match($SignatureLine, '(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(')
    if ($match.Success) {
        return $match.Groups['name'].Value
    }

    return $null
}

function Is-MethodSignatureLine {
    param(
        [Parameter(Mandatory = $true)][string]$Line
    )

    $trimmed = $Line.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return $false
    }

    if ($trimmed.StartsWith('[') -or $trimmed.StartsWith('#') -or $trimmed.StartsWith('//')) {
        return $false
    }

    $invalidStarts = @(
        'using ',
        'namespace ',
        'class ',
        'record ',
        'interface ',
        'enum ',
        'struct ',
        'delegate ',
        'if ',
        'for ',
        'foreach ',
        'while ',
        'switch ',
        'catch ',
        'return ',
        'throw '
    )

    foreach ($prefix in $invalidStarts) {
        if ($trimmed.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            return $false
        }
    }

    $methodPattern = '^(?:(?:public|private|protected|internal|static|virtual|override|abstract|sealed|async|extern|new|unsafe|partial|required)\s+)+[A-Za-z_][A-Za-z0-9_<>,\[\]\.\?\s]*\s+[A-Za-z_][A-Za-z0-9_]*\s*\([^;]*\)\s*(?:where\s+.+)?(?:=>|\{|$)'
    $constructorPattern = '^(?:(?:public|private|protected|internal|static|extern|unsafe|partial)\s+)+[A-Za-z_][A-Za-z0-9_]*\s*\([^;]*\)\s*(?:where\s+.+)?(?:=>|\{|$)'

    return ([System.Text.RegularExpressions.Regex]::IsMatch($trimmed, $methodPattern) -or [System.Text.RegularExpressions.Regex]::IsMatch($trimmed, $constructorPattern))
}

function Test-SignatureChangeInFile {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeFilePath,
        [Parameter(Mandatory = $true)][string]$WorkspaceRoot
    )

    $diffOutput = & git -c core.quotepath=false diff --unified=0 -- $RelativeFilePath 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($diffOutput | Out-String))) {
        return $false
    }

    $removedByMethod = @{}
    $addedByMethod = @{}

    foreach ($line in $diffOutput) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line.StartsWith('---') -or $line.StartsWith('+++') -or $line.StartsWith('@@')) {
            continue
        }

        if ($line.StartsWith('-')) {
            $candidate = $line.Substring(1)
            if (-not (Is-MethodSignatureLine -Line $candidate)) {
                continue
            }

            $name = Try-ExtractMethodName -SignatureLine $candidate
            if ([string]::IsNullOrWhiteSpace($name)) {
                continue
            }

            if (-not $removedByMethod.ContainsKey($name)) {
                $removedByMethod[$name] = [System.Collections.Generic.List[string]]::new()
            }

            $removedByMethod[$name].Add($candidate.Trim())
            continue
        }

        if ($line.StartsWith('+')) {
            $candidate = $line.Substring(1)
            if (-not (Is-MethodSignatureLine -Line $candidate)) {
                continue
            }

            $name = Try-ExtractMethodName -SignatureLine $candidate
            if ([string]::IsNullOrWhiteSpace($name)) {
                continue
            }

            if (-not $addedByMethod.ContainsKey($name)) {
                $addedByMethod[$name] = [System.Collections.Generic.List[string]]::new()
            }

            $addedByMethod[$name].Add($candidate.Trim())
        }
    }

    foreach ($methodName in $removedByMethod.Keys) {
        if (-not $addedByMethod.ContainsKey($methodName)) {
            continue
        }

        foreach ($oldSignature in $removedByMethod[$methodName]) {
            foreach ($newSignature in $addedByMethod[$methodName]) {
                if ($oldSignature -ne $newSignature) {
                    return $true
                }
            }
        }
    }

    return $false
}

function Resolve-ProjectForFile {
    param(
        [Parameter(Mandatory = $true)][string]$RelativeFilePath,
        [Parameter(Mandatory = $true)][string]$WorkspaceRoot
    )

    $absoluteFilePath = Join-Path $WorkspaceRoot $RelativeFilePath
    $directory = Split-Path -Parent $absoluteFilePath
    $workspaceResolved = [System.IO.Path]::GetFullPath($WorkspaceRoot)

    while (-not [string]::IsNullOrWhiteSpace($directory)) {
        $resolvedDirectory = [System.IO.Path]::GetFullPath($directory)
        if (-not $resolvedDirectory.StartsWith($workspaceResolved, [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }

        $project = Get-ChildItem -LiteralPath $resolvedDirectory -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $project) {
            return [System.IO.Path]::GetRelativePath($WorkspaceRoot, $project.FullName).Replace('\\', '/')
        }

        $parent = Split-Path -Parent $resolvedDirectory
        if ([string]::IsNullOrWhiteSpace($parent) -or $parent -eq $resolvedDirectory) {
            break
        }

        $directory = $parent
    }

    return $null
}

$rawInput = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($rawInput)) {
    exit 0
}

try {
    $payload = $rawInput | ConvertFrom-Json -Depth 100
}
catch {
    # Non-blocking for malformed payloads.
    exit 0
}

$workspaceRoot = (Get-Location).Path

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    exit 0
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    exit 0
}

$editToolNames = @(
    'apply_patch',
    'create_file',
    'replace_string_in_file',
    'multi_replace_string_in_file',
    'edit_file',
    'delete_file',
    'rename_file',
    'move_file'
)

$toolNameCandidates = Collect-ValuesByPropertyName -InputObject $payload -PropertyNames @('toolName', 'tool_name', 'tool')
$toolName = $toolNameCandidates | Where-Object { $_ -is [string] -and -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1

if ($toolName) {
    $normalizedToolName = $toolName.ToString().Trim().ToLowerInvariant()
    if (-not ($editToolNames -contains $normalizedToolName)) {
        exit 0
    }
}

$pathValues = Collect-ValuesByPropertyName -InputObject $payload -PropertyNames @('filePath', 'path', 'new_path', 'old_path')
$rawPaths = [System.Collections.Generic.List[string]]::new()

foreach ($value in $pathValues) {
    if ($value -is [string]) {
        $rawPaths.Add($value)
    }
}

$patchInputs = Collect-ValuesByPropertyName -InputObject $payload -PropertyNames @('input')
foreach ($patchInput in $patchInputs) {
    if (-not ($patchInput -is [string])) {
        continue
    }

    $matches = [System.Text.RegularExpressions.Regex]::Matches(
        $patchInput,
        '(?m)^\*\*\*\s+(?:Update|Add|Delete)\s+File:\s+(.+)$')

    foreach ($match in $matches) {
        if ($match.Groups.Count -gt 1) {
            $rawPaths.Add($match.Groups[1].Value)
        }
    }
}

$csharpFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

foreach ($rawPath in $rawPaths) {
    $relativePath = Get-NormalizedRelativePath -Path $rawPath -WorkspaceRoot $workspaceRoot
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        continue
    }

    if (-not $relativePath.EndsWith('.cs', [System.StringComparison]::OrdinalIgnoreCase)) {
        continue
    }

    $absolutePath = Join-Path $workspaceRoot $relativePath
    if (Test-Path -LiteralPath $absolutePath) {
        [void]$csharpFiles.Add($relativePath)
    }
}

if ($csharpFiles.Count -eq 0) {
    exit 0
}

$signatureChangedFiles = [System.Collections.Generic.List[string]]::new()
foreach ($file in $csharpFiles) {
    if (Test-SignatureChangeInFile -RelativeFilePath $file -WorkspaceRoot $workspaceRoot) {
        $signatureChangedFiles.Add($file)
    }
}

if ($signatureChangedFiles.Count -eq 0) {
    exit 0
}

$projectsToBuild = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($file in $signatureChangedFiles) {
    $projectPath = Resolve-ProjectForFile -RelativeFilePath $file -WorkspaceRoot $workspaceRoot
    if (-not [string]::IsNullOrWhiteSpace($projectPath)) {
        [void]$projectsToBuild.Add($projectPath)
    }
}

if ($projectsToBuild.Count -eq 0) {
    $defaultProject = 'HotelStay.API/HotelStay.API.csproj'
    if (Test-Path -LiteralPath (Join-Path $workspaceRoot $defaultProject)) {
        [void]$projectsToBuild.Add($defaultProject)
    }
}

if ($projectsToBuild.Count -eq 0) {
    exit 0
}

$failedProjects = [System.Collections.Generic.List[string]]::new()
$orderedProjects = $projectsToBuild.ToArray() | Sort-Object

foreach ($projectPath in $orderedProjects) {
    Write-Host "Running dotnet build $projectPath because method signatures changed."
    & dotnet build $projectPath --nologo

    if ($LASTEXITCODE -ne 0) {
        $failedProjects.Add($projectPath)
    }
}

if ($failedProjects.Count -gt 0) {
    $joinedProjects = ($failedProjects | Sort-Object | Get-Unique) -join ', '
    $message = "dotnet build failed after C# method signature or return type changes in: $joinedProjects"

    $output = @{
        decision = 'block'
        reason = $message
        systemMessage = $message
    }

    $output | ConvertTo-Json -Depth 5 -Compress
    exit 2
}

exit 0
