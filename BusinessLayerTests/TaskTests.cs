using IntroSE.Kanban.Backend.BusinessLayer;
using Task = IntroSE.Kanban.Backend.BusinessLayer.Task;

namespace BusinessLayerTests
{
    public  class TaskTests
    {
        private Task testTask;

        [SetUp]
        public void Setup()
        {
            testTask = new Task(0,1, DateTime.Now, DateTime.Now.AddDays(7).Date, "Feed dog", "This is a test task description");
        }

        [Test]
        public void Constructor_ValidParameterss()
        {
            Assert.That(testTask.TaskID, Is.EqualTo(1));
            Assert.That(testTask.DueDate.Date, Is.EqualTo(DateTime.Now.AddDays(7).Date));
            Assert.That(testTask.Title, Is.EqualTo("Feed dog"));
            Assert.That(testTask.Description, Is.EqualTo("This is a test task description"));
        }

        [Test]
        public void UpdateTaskTitle_ValidTitle()
        {
            string newTitle = "Updated Task Title";

            testTask.UpdateTaskTitle(newTitle);

            Assert.That(testTask.Title, Is.EqualTo(newTitle));
        }

        [Test]
        public void UpdateTaskTitle_NullTitle_ThrowsException()
        {
            string nullTitle = null;

            Assert.Throws<ArgumentException>(() => testTask.UpdateTaskTitle(nullTitle));
        }

        [Test]
        public void UpdateTaskTitle_EmptyTitle_ThrowsException()
        {
            string emptyTitle = string.Empty;

            Assert.Throws<ArgumentException>(() => testTask.UpdateTaskTitle(emptyTitle));
        }

        [Test]
        public void UpdateTaskTitle_TitleTooLong_ThrowsException()
        {
            string longTitle = "";
            for(int i = 0; i < 51; i++)
                longTitle += i.ToString();

            Exception exception = Assert.Throws<ArgumentException>(() => testTask.UpdateTaskTitle(longTitle));
            Assert.That(exception.Message, Is.EqualTo("Title too long."));
        }

        [Test]
        public void UpdateTaskDescription_ValidDescription()
        {
            string newDescription = "Don't feed the dog.";
            testTask.UpdateTaskDescription(newDescription);
            Assert.That(testTask.Description, Is.EqualTo(newDescription));
        }

        [Test]
        public void UpdateTaskDescription_DescriptionTooLong_ThrowsException()
        {
            string longDescription = "";
            for (int i = 0; i < 301; i++)
                longDescription += i.ToString();

            Exception exception = Assert.Throws<ArgumentException>(() => testTask.UpdateTaskDescription(longDescription));
            Assert.That(exception.Message, Is.EqualTo("Description too long."));
        }

        [Test]
        public void UpdateTaskDescription_EmptyDescription_Valid()
        {
            string emptyDescription = string.Empty;
            testTask.UpdateTaskDescription(emptyDescription);

            Assert.That(testTask.Description, Is.EqualTo(emptyDescription));
        }

        [Test]
        public void UpdateTaskDescription_NullDescription_ThrowsException()
        {
            string nullDescription = null;

            Exception exception = Assert.Throws<ArgumentException>(() => testTask.UpdateTaskDescription(nullDescription));
            Assert.That(exception.Message, Is.EqualTo("Description can't be null."));
        }

        [Test]
        public void UpdateTaskDueDate_ValidFutureDate_UpdatesDueDate()
        {
            DateTime newDueDate = DateTime.Now.AddDays(14);
            testTask.UpdateTaskDueDate(newDueDate);

            Assert.That(testTask.DueDate, Is.EqualTo(newDueDate));
        }

        [Test]
        public void UpdateTaskDueDate_PastDate_ThrowsException()
        {
            DateTime pastDate = DateTime.Now.AddDays(-1);

            Exception exception = Assert.Throws<ArgumentException>(() => testTask.UpdateTaskDueDate(pastDate));
            Assert.That(exception.Message, Is.EqualTo("Due date invalid."));
        }

        [Test]
        public void UpdateTaskDueDate_CurrentDate_ThrowsException()
        {
            DateTime currentDate = DateTime.Now.AddSeconds(-1);

            Exception exception = Assert.Throws<ArgumentException>(() => testTask.UpdateTaskDueDate(currentDate));
            Assert.That(exception.Message, Is.EqualTo("Due date invalid."));
        }
    }
}
