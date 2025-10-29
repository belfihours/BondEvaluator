# Bond Evaluator
Welcome to Bond Evaluator, a simple REST API that returns bond evaluations based on input csv.
The output will be another csv file containing [BondID, Type, PresentValue] (mandatory ones) + [Issuer, Rating, DeskNotes] (this data can be useful in investment meetings)
Since I think less data it's better than no data at all, I decided to just skip all the csv rows that could be corrupted or not correctly parsed, logging the problem.
This can be easily reverted in case of different businesses requirements.

# Usage
You can clone code and run API with an IDE (e.g. Visual Studio, Rider), or simpler just run the Docker image

## How to run Docker image
Download [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/), clone repository, open terminal in repository folder and run
```cmd
docker build -f BondEvaluator.API/Dockerfile -t bond-evaluator .
```
After successfully build, run
```cmd
docker run -d -p 8080:8080 -p 8081:8081 --name bond-evaluator bond-evaluator
```
After that, you can start and stop the container with
```cmd
docker <start|stop> bond-evaluator
```

## Example
You can call the endpoint
http://localhost:8080/bondevaluator/bondevaluations with a POST request, attaching the csv file in form-data body.
A csv example is avilable [inside the solution](BondEvaluator.API.IntegrationTest/TestData/bond_positions_sample.csv)

# Used dependencies
### Logging-related packages
- Microsoft.logging.abstractions
- Serilog
### Test-related packages
- Swagger 
- Moq
- Fixture
- Microsoft.AspNetCore.Mvc.Testing
# Possible improvements
- Change the current logging system (in a simple file) to cloud services like [Azure Monitor Logs](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/data-platform-logs) or [Datadog](https://www.datadoghq.com/)
- Change API system to an event based one, so that the whole process is automatic
- Add a connector in order to automatically update InflationRate (currently based on [2024 data](https://www.worlddata.info/europe/netherlands/inflation-rates.php#:~:text=The%20inflation%20rate%20for%20consumer%20prices%20in%20the,per%20year.%20Overall%2C%20the%20price%20increase%20was%20747.44%25.))
- Add FluentAssertion package for more readable tests
