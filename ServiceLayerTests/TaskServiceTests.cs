using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using IntroSE.Kanban.Backend.ServiceLayer.Entities;
using IntroSE.Kanban.Backend.ServiceLayer.Services;
using NUnit.Framework.Internal;


/**
 * Black-box tests for the Service Layer.
 * NUnit is used for convenience; the goal is to validate external behavior,
 * not to perform white-box testing of the Service Layer classes.
 **/

namespace ServiceLayerTests
{
    public class TaskServiceTests
    {
        private ServiceFactory serviceFactory;

        [SetUp]
        public void Setup()
        {
            serviceFactory = new ServiceFactory();

            TearDown();

            serviceFactory.UserService.Register("test@gmail.com", "Test123");
            serviceFactory.BoardService.CreateBoard("test@gmail.com", "MyBoard");
            serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Feed dog", "Give the dog food in 3 hours", DateTime.Now.AddHours(3));
            serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "test@gmail.com");
        }

        [TearDown]
        public void TearDown()
        {
            serviceFactory.DeleteData();
        }

        [Test]
        public void AddTask()
        {
            // Test 1 - Valid input
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "TitleTest", "TestDescription", DateTime.Now.AddHours(3));
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - BoardName not valid
            ans = serviceFactory.TaskService.AddTask("test@gmail.com", "NotMyBoard", "Feed dog", "Give the dog food in 3 hours", DateTime.Now.AddHours(3));
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);
            
            // Test 3 - Email does not exist
            ans = serviceFactory.TaskService.AddTask("test@yahoo.com", "MyBoard", "Feed dog", "Give the dog food in 3 hours", DateTime.Now.AddHours(3));
            Response<string> response4 = JsonSerializer.Deserialize<Response<string>>(ans);
            
            Assert.True(
                response1.ErrorMessage == null &&
                response2.ErrorMessage == "Board doesnt exist." &&
                response4.ErrorMessage == "User not found." );
        }

        [Test]
        public void AdvanceTask()
        {
            //Test 1 - Valid input
            string ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "MyBoard", 0, 0);
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Task doesn't exist
            ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "MyBoard", 0, 1);
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 3 - BoardName not valid
            ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "NotMyBoard", 0, 0);
            Response<string> response3 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 4 - Email doesn't exist
            ans = serviceFactory.TaskService.AdvanceTask("test@yahoo.com", "MyBoard", 0, 1);
            Response<string> response4 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 6 - Task advance valid (a task in DONE column cannot be advanced)
            ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "MyBoard", 1, 0);
            Response<string> response5 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 7 - Task advance not valid (a task in DONE column cannot be advanced)
            ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "MyBoard", 2, 0);
            Response<string> response6 = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                   response1.ErrorMessage == null &&
                   response2.ErrorMessage == "Task with ID 1 not found in the column." &&
                   response3.ErrorMessage == "Board doesnt exist." &&
                   response4.ErrorMessage == "User not found." &&
                   response5.ErrorMessage == null &&
                   response6.ErrorMessage == "Cannot advance a task from the 'done' column.");
        }

        [Test]
        public void UpdateTaskDescription()
        {
            // Test 1 - Valid input
            string ans = serviceFactory.TaskService.UpdateTaskDescription("test@gmail.com", "MyBoard", 0, 0, "Updated Description");
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Task doesn't exist
            ans = serviceFactory.TaskService.UpdateTaskDescription("test@gmail.com", "MyBoard", 0, 1, "Updated Description");
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 3 - Task is completed
            ans = serviceFactory.TaskService.UpdateTaskDescription("test@gmail.com", "MyBoard", 2, 0, "Feed dog again, he's skinny");
            Response<string> response3 = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                   response1.ErrorMessage == null &&
                   response2.ErrorMessage == "Task with ID 1 not found in the column." &&
                   response3.ErrorMessage == "Task with ID 0 not found in the column.");
        }

        [Test]
        public void UpdateTaskTitle()
        {
            // Test 1 - Valid input
            string ans = serviceFactory.TaskService.UpdateTaskTitle("test@gmail.com", "MyBoard", 0, 0, "Updated Title");
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Invalid Title
            ans = serviceFactory.TaskService.UpdateTaskTitle("test@gmail.com", "MyBoard", 0, 0, "");
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 3 - Task is completed
            ans = serviceFactory.TaskService.UpdateTaskTitle("test@gmail.com", "MyBoard", 2, 0, "Feed dog again");
            Response<string> response3 = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                   response1.ErrorMessage == null &&
                   response2.ErrorMessage == "Task title can't be null or empty." &&
                   response3.ErrorMessage == "Task with ID 0 not found in the column.");
        }

        [Test]
        public void UpdateTaskDueDate()
        {
            // Test 1 - Valid input
            string ans = serviceFactory.TaskService.UpdateTaskDueDate("test@gmail.com", "MyBoard", 0, 0, DateTime.Now.AddDays(1));
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Invalid Date
            ans = serviceFactory.TaskService.UpdateTaskDueDate("test@gmail.com", "MyBoard", 0, 0, DateTime.Now.AddDays(-1));
            Response<TaskSL> response2 = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            Assert.True(
                   response1.ErrorMessage == null &&
                   response2.ErrorMessage == "Due date invalid.");
        }

        [Test]
        public void InProgressTasksTest()
        {
            // Test 1 - Valid input
            string ans = serviceFactory.TaskService.InProgressTasks("test@gmail.com");
            Response<List<TaskSL>> response1 = JsonSerializer.Deserialize<Response<List<TaskSL>>>(ans);

            // Test 2 - Invalid email
            ans = serviceFactory.TaskService.InProgressTasks("wrong@gmail.com");
            Response<List<TaskSL>> response2 = JsonSerializer.Deserialize<Response<List<TaskSL>>>(ans);

            Assert.IsTrue( 
                   response1.ErrorMessage == null &&
                   response2.ErrorMessage == "User not found." );
        }

        [Test]
        public void AssignTask_OnlyAssigneeCanReassign()
        {
            // Test 1 - Non-assignee tries to reassign (should fail)
            serviceFactory.UserService.Register("assignee@gmail.com", "Test123");
            serviceFactory.BoardService.JoinBoard("assignee@gmail.com", 1);
            string ans = serviceFactory.TaskService.AssignTask("assignee@gmail.com", "MyBoard", 0, 0, "assignee@gmail.com");
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Current assignee reassigns (should succeed)
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "assignee@gmail.com");
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                response1.ErrorMessage == "assignee@gmail.com is not the assigner of this task." &&
                response2.ErrorMessage == null
            );
        }

        [Test]
        public void AssignTask_AssigneeDoesNotExist()
        {
            // Test 1 - Create a new unassigned task
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Unassigned Task", "No assignee", DateTime.Now.AddHours(2));
            Response<string> addResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Try to assign the new task to a non-existent user
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "notfound@gmail.com");
            Response<string> assignResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                addResponse.ErrorMessage == null &&
                assignResponse.ErrorMessage == "User not found."
            );
        }

        [Test]
        public void AssignTask_AssigneeNotBoardMember()
        {
            // Test 1 - Create a new unassigned task
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Unassigned Task", "No assignee", DateTime.Now.AddHours(2));
            Response<string> addResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Register outsider (not a board member)
            serviceFactory.UserService.Register("outsider@gmail.com", "Test123");

            // Test 3 - Try to assign the new task to outsider
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "outsider@gmail.com");
            Response<string> assignResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                addResponse.ErrorMessage == null &&
                assignResponse.ErrorMessage == "User 'outsider@gmail.com' is not a member of the board 'MyBoard'."
            );
        }


        [Test]
        public void UpdateTaskDescription_OnlyAssigneeCanChange()
        {
            serviceFactory.UserService.Register("assignee@gmail.com", "Test123");
            serviceFactory.BoardService.JoinBoard("assignee@gmail.com", 1);

            // Test 1 - Assignee updates the description
            string ans = serviceFactory.TaskService.UpdateTaskDescription("test@gmail.com", "MyBoard", 0, 0, "Assignee update");
            Response<string> response1 = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 2 - Non-assignee tries to update
            ans = serviceFactory.TaskService.UpdateTaskDescription("assignee@gmail.com", "MyBoard", 0, 0, "Owner update");
            Response<string> response2 = JsonSerializer.Deserialize<Response<string>>(ans);

            Assert.True(
                response1 != null && response1.ErrorMessage == null &&
                response2 != null && response2.ErrorMessage == "assignee@gmail.com not the assignee of this task."
            );
        }

        [Test]
        public void InProgressTasks_OnlyAssignedTasksReturned()
        {
           serviceFactory.UserService.Register("assignee@gmail.com", "Test123");

            // Test 1 - Get in-progress tasks for the assignee
            string ans = serviceFactory.TaskService.InProgressTasks("assignee@gmail.com");
            Response<List<TaskSL>> response = JsonSerializer.Deserialize<Response<List<TaskSL>>>(ans);

            // Test 2 - All returned tasks are assigned to the assignee
            Assert.IsTrue(response.ErrorMessage == null && response.ReturnValue.All(t => t.Assignee == "assignee@gmail.com"));
        }

        [Test]
        public void AssignTask_UnassignedAndAssignedBehavior()
        {
            serviceFactory.UserService.Register("assignee@gmail.com", "Test123");
            // Join assignee to the board
            serviceFactory.BoardService.JoinBoard("assignee@gmail.com", 1);

            // Create a new unassigned task (should be taskID 1)
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Unassigned Task", "No assignee", DateTime.Now.AddHours(2));
            Response<string> addResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            // Test 1 - Assign unassigned task as board member
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "assignee@gmail.com");
            Response<TaskSL> response1 = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            // Test 2 - Try to reassign as non-assignee (should fail)
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "test@gmail.com");
            Response<TaskSL> response2 = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            // Test 3 - Reassign as assignee (should succeed)
            ans = serviceFactory.TaskService.AssignTask("assignee@gmail.com", "MyBoard", 0, 0, "test@gmail.com");
            Response<TaskSL> response3 = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            Assert.True(
                addResponse.ErrorMessage == null &&
                response1.ErrorMessage == null &&
                response2.ErrorMessage == "test@gmail.com is not the assigner of this task." &&
                response3.ErrorMessage == null
            );
        }

        [Test]
        public void AdvanceTask_AssigneeCanAdvance_Succeeds()
        {
            // Arrange: Register a new assignee and join them to the board
            serviceFactory.UserService.Register("assignee@gmail.com", "Test123");
            serviceFactory.BoardService.JoinBoard("assignee@gmail.com", 1);

            // Arrange: Create a new unassigned task (should be taskID 1)
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Unassigned Task", "No assignee", DateTime.Now.AddHours(2));
            Response<string> addResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            // Assign the task to the assignee
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "assignee@gmail.com");
            Response<TaskSL> assignResponse = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            // Act: Assignee advances the task
            ans = serviceFactory.TaskService.AdvanceTask("assignee@gmail.com", "MyBoard", 0, 0);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(ans);

            // Assert: Should succeed
            Assert.True(
                addResponse.ErrorMessage == null &&
                assignResponse.ErrorMessage == null &&
                response.ErrorMessage == null
            );
        }

        [Test]
        public void AdvanceTask_NonAssigneeCannotAdvance_Fails()
        {
            // Arrange: Register and join assignee to the board
            serviceFactory.UserService.Register("assignee@gmail.com", "Test123");
            serviceFactory.BoardService.JoinBoard("assignee@gmail.com", 1);

            // Arrange: Create a new unassigned task (should be taskID 1)
            string ans = serviceFactory.TaskService.AddTask("test@gmail.com", "MyBoard", "Unassigned Task", "No assignee", DateTime.Now.AddHours(2));
            Response<string> addResponse = JsonSerializer.Deserialize<Response<string>>(ans);

            // Assign the task to the assignee
            ans = serviceFactory.TaskService.AssignTask("test@gmail.com", "MyBoard", 0, 0, "assignee@gmail.com");
            Response<TaskSL> assignResponse = JsonSerializer.Deserialize<Response<TaskSL>>(ans);

            // Act: Non-assignee tries to advance the task
            ans = serviceFactory.TaskService.AdvanceTask("test@gmail.com", "MyBoard", 0, 0);
            Response<string> response = JsonSerializer.Deserialize<Response<string>>(ans);

            // Assert: Should fail with appropriate error
            Assert.True(
                addResponse.ErrorMessage == null &&
                assignResponse.ErrorMessage == null &&
                response.ErrorMessage == "test@gmail.com not the assignee of this task."
            );
        }
    }
}