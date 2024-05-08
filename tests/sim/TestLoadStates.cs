namespace tests;

using System.IO.Abstractions;
using Moq;
using sim;


public class TestLoadStates
{

    [Fact]
    public void testLoad()
    {
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.File.ReadAllText(It.IsAny<string>())).Returns("{\"StartUpEvent\":{\"Name\":\"StartUpEvent\",\"Transitions\":{\"DocumentChangeEvent\":1}},\"DocumentChangeEvent\":{\"Name\":\"DocumentChangeEvent\",\"Transitions\":{}}}");
        var loader = new LoadStates(fileSystemMock.Object);

        var state = loader.Load("test.json");

        Assert.Equal(state.Name, EventName.StartUpEvent);
        Assert.Equal(state.GetNextState().Name, EventName.DocumentChangeEvent);
    }

    [Fact]
    public void testLoadWithEmptyFile()
    {
        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(fs => fs.File.ReadAllText(It.IsAny<string>())).Returns("{}");
        var loader = new LoadStates(fileSystemMock.Object);

        Assert.Throws<InvalidOperationException>(() => loader.Load("test.json"));

    }
}
