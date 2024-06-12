# Dotnet testing automation framework using a test management system
### Component diagram

![Image_ComponentDiagram](Readme/ComponentDiagram.PNG)


### Test agent
- WebSocket client;
- Execute bash comand for start test execution (dotnet text --filter "Name~{testId}");
![TestAgent_run_command](Readme/TestAgent_run_command.PNG)
- Load Allure report result from test build directory into TMS;


![Image_TestAgentBPMN](Readme/TestAgentBusinessProcessDiagramm.PNG)

### Test Server
- WebSocket server;
- Kafka Consumer;

![Test agent server](Readme/TestAgentServer.PNG)

### Overview
The work is part of a master's thesis. Its full text can be found in the readme section.
