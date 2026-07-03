## Prompts used:

```md
/create-agent Create an agent which will read a `PRD.MD` or product requirement file [mostly markdown file], then it will try to generate a `spec.md` file.

Spec.md file must contain :
- Problem Statement section with business context subsections
- Scope section with in scope and out of scope subsections with detailed bold[ highlighted keywords] points
- API project architecture [ possible ] with project structure and kept for discussion, UI [ if there]
- Data models 
- Interface contracts, endpoints with little details, like req , response, query param if there, success, error both response
- UI components [ if there in PRD ] also ask user which UI framwork like to use
- Acceptance criteria , each acceptance criteria must have `given`, `when`, `then`
- Error handling with response format, UI error handling
- Assumptions in different aspects
- How to Validate the Specs with Tests for API with proper test method naming liek 'methodname_condition_result` liek that
- Summary

```
---

```md
Here is the #file:PRD.md file , lets generate the spec file accordingly.
```

---

```md
Now since we are not using any external API provider, we need to use a mock way which will provide the data.
SO we will use a json file with those data , next providers will read from these and send back to us. So while desginng this part will keep in mind this should be extensible in future.

Second thing while booking as of now we are not using any persistant , so for persisting will desing in such a way there should not be any difficulites if we want to switch to differnt db providers. lets think and discuss first before updating spec file
```

```md
I already have the API project, now lets start implementing according to the `spec` file.
NOTE: As of now will focus on PAI only, for test will do after that.
```

---

```md
After careful review I see lot of improvements scopes :
- we should use already available IExceptionHandler interface to implement global exceptions and registering properly in program.cs file
- for validating docuemt we can use strategy pattern to handle differnt validations for different docuemtn type, so one meanigul interface then implementing that
- for problemdetails factory we dont require to implemetn explcitle, dotnet has this in built 
- also for booking service, search service, we should use interface for that to implemtn, so while testing it will be easy

Lets address one by one 

```

---

```md
Ohkay, now I see some improvements , also after making some changes manully,the following things we can still improve :
- in endpoints, I see we are creating problem details manually, we should avoid that , that is for exceptions, from spec we have common APIResponse for both success and failure , so we need to update the service to have that return type so we can handle the error explicitly, so in endpoint it will be clear
- I see namespace is used at the top under usings, in some files, we should make it constant where usings will be at the top always
- next lets add one more endpoint which will get the booking details by the refernce `/api/bookings/{booking.BookingReference}` which we can see we are passing in create booking endpoint.

Lets address these now

```

---

```md
- add getbooking in http file also
- in endpoint classes I see lots of thing at once, we should use the result type using Map method and create a proper Results return type, and other part like validation make it separete method and return result type after validating , that may contain success as bool or errors , if errors then will return results.badrequest like that.

Lets fix the endpoints

```

---

```md
Lets now add tests 4-5 for each for the #file:InMemoryBookingRepository.cs #sym:FlightSearchService #sym:BookingService , test must contain both valid and invalid cases meaningfully.

```

---

```md
Also add `ITestOutputHelper` and for each test method use that to console what the test is testing and what will be the expected

```

---

```md
Here are the endpoints and UI project already scaffolded, Now lets implement the UI adhering the #file:PRD.md , how the UI should be. And the spec file #file:spec.md for detailed overview.

NOTE: 
- Use template html file, then ts file file for business logic, scss file for styling
- Use dark mode as default 

```

---

```md
- Instead using one single api service, we can break into separate and also for booking service will add one more to get booking details by refernce number and its corresponding component.
- And also keep the services minimal, like dont unwrap in service, use in component ts to file to handle error gracefully
```

---

```md

Lets now create the project root `README.md` in such a way that it covers following points :
- setup and run instructions for both API and UI in a separate sub sections
- a brief description of the architecture
    - context : The API project mostly follows vertical slice architecture where features are separated and have shared folder also which can be shared accross. Also for extensibility, if we want to add new functionality of feature we can do that like user, authentication, email notifications etc
- Also for error handling we have created a simplified custom api response type to handle both success and failure explicitly and for any exceptions we have `GlobalExceptionHandler` with standard RFC problem details.

- Some trade-offs / limitations
    - The custom API response can be more enhanced in terms of proper `Error` structure and metadata
    - Also, we can use and organize the errors well and in the UI can be handled properly
    - Also it doesn't have any logic of total seats, available seats, seat choosing, adding coupons etc
    - Add others as well

- At the end add a future scopes like 
    - Adding authentication and authorization
    - Realtime external providers
    - User data, and persisting in some database
    - Realtime notifications
    - Adding structured logs
    - Enlisting `Aspire` integration to have a detaild dashboard for both API and UI

> Add little more detailed set up and run like from git clone and changin to proper directory
    for trade offs and limitations and also for future scope also add little more details with sub points kind of
```

---
