namespace tests;

using Moq;
using dataProcessor;

using System.IO.Abstractions;

public class TestLoader {

    private readonly string _testEvent = "[{\"EventName\":\"DocumentChangeEvent\",\"eventTime\":1715086246152,\"sessionId\":\"41143742-82a2-4955-b16d-78d61048b6091715085535824\",\"MachineId\":\"6dc582831b8d88e52c3c496117591e7c67ab54528dff6018895d4871bdd18f85\",\"documentUri\":\"file:///media/ssd2/dev/HSLU/rt/src/scene.h\",\"documentId\":2,\"operation\":\"add\",\"changeOperation\":{\"text\":\"a\",\"rangeOffset\":\"841\",\"rangeLength\":\"0\",\"rangestart_line\":\"31\",\"rangestart_character\":\"28\",\"rangeend_line\":\"31\",\"rangeend_character\":\"28\"}}]";

    [Fact]
    public void TestLoad() {
        var mock = new Mock<IFileSystem>();
        mock.Setup(loader => loader.File.ReadAllText("")).Returns(_testEvent);
        var loader = new Loader(mock.Object);
        var data = loader.Load("");
        Assert.Equal(EventName.DocumentChangeEvent, data[0].EventName);
        Assert.Equal(1715086246152, data[0].EventTime);
        Assert.Equal("41143742-82a2-4955-b16d-78d61048b6091715085535824", data[0].SessionId);
        Assert.Equal("6dc582831b8d88e52c3c496117591e7c67ab54528dff6018895d4871bdd18f85", data[0].MachineId);
        Assert.Equal("file:///media/ssd2/dev/HSLU/rt/src/scene.h", data[0].DocumentUri?.ToString());
        Assert.Equal(2, data[0].documentId);
        Assert.Equal(Operation.add, data[0].Operation);
        Assert.Equal("a", data[0].ChangeOperation?.text);
        Assert.Equal("841", data[0].ChangeOperation?.rangeOffset);
        Assert.Equal("0", data[0].ChangeOperation?.rangeLength);
        Assert.Equal("31", data[0].ChangeOperation?.RangeStart_Line);
        Assert.Equal("28", data[0].ChangeOperation?.RangeStart_Character);
        Assert.Equal("31", data[0].ChangeOperation?.RangeEnd_Line);
        Assert.Equal("28", data[0].ChangeOperation?.RangeEnd_Character);
    }
}
