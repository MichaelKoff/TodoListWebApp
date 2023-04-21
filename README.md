## Project Description   

The project will allow you to solidify your proficiency in C# and ASP.NET Core. As you design your web application, you will get a practical experience of creating domain models, developing unit tests, and implementing a user interface and the presentation layer using .NET Core and ASP.NET Core. When you complete this project, you will have a working web application showpiece to include in your portfolio.

The goal of the project is to create a To Do List web application that allows users to manage their personal activities by creating to do lists. The web application should work correctly on a desktop computer and a mobile phone. You may use the domain model from the Week #10 assignment as a starting point for your design. However, this application should be more functional. 

You are encouraged to check out [this application](https://www.microsoft.com/ru-ru/microsoft-365/microsoft-to-do-list-app?rtc=1 ) as an example in order to get a general understanding of the desired result. However, please do not copy the design of the Microsoft application. Try to create your own design and functionality. 

To finish the project, you will have about 40 hours. To guide you through the process, the project implementation is divided into three stages: 
1. Creating the first prototype of the app with basic functionality. 
1. Creating the final variant of the app with more advanced functionality and UI design. 
1. Getting ready for a live presentation of the final solution. 

## Stage 1. Creating an App Prototype   

At this stage, you are expected to create the first prototype of your application that meets the stated requirements. 

**Project  Solution Requirements**

The solution should be multi-project containing at least three VS projects:

- A class library project which contains classes that represent a domain model for the To Do List application.
- A client ASP.NET Core MVC application that end users can use to interact with the To Do List application.
- A unit test project that provides unit tests for the To Do List class library.

The VS solution may look as follows:

![](images/todo-solition.png) 

**Project Architecture  Requirements**

- All the To Do List application data is stored in the database. 
- The EF Core or other ORMs to save and read data from the database can be used.  
- The unit test project and unit tests have to be created for the domain model. 

You may use this application as your portfolio project, so it is highly recommended to deploy it to the Azure Cloud. 

**Functionality Requirements**

At this stage, you are expected to build the functionalities that will enable your users to: 

- view all To Do lists at the application’s list view  
- create new To Do lists   
- add To Do entries (items) to a To Do list   
- enter a title, a description, a due date, a creation date to each To Do item  
- change To Do items status to: Completed, In Progress, Not Started  
- modify a To Do list or a To Do entry 

## Stage 2. Building the Final Version 
 
**Functionality Requirements**

At this stage, you are expected to create the functionalities that will enable your users to: 

- store all To Do List application data in a database  
- show/hide completed items in a To Do list  
- copy the existing To Do list  
- remove/hide a list from the application’s list view  
- add additional fields/notes to To Do items  
- hide a To Do list from the list view or remove it completely from the TODO list database  
- see the To Do items that are due today  
- create a reminder for a To Do item 

**UI Design Requirements**

You are also offered to create your UI design for the web application. Here is a reference UI design: 

![](images/Net17_Pic2_small_3x.png) 
 
## Stage 3. Presenting the Final Solution 

After you are ready with your solution, prepare for a live presentation.  

**Deployment requirements:**

- Commit and push your latest working version to GitLab to present the application to your mentor in live mode by sharing your screen. Create a free user account at Microsoft Azure, deploy your project to the MS Azure Cloud, and provide a URL to your mentor (optional). 

