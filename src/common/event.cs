namespace common;


public enum EventName
{
    DocumentOpenEvent,
    DocumentChangeEvent,
    DocumentCloseEvent,
    DocumentSaveEvent,
    AddCharcterEvent,
    DeleteCharcterEvent,
    StartUpEvent,
    GitEvent,
    RunEvent,
}

public enum Operation
{
    pull,
    commit,
    checkout,
    add,
    replace,
    delete,
    save,
    open,
    close,
    startUp,
    start,
    terminate,
}

public class ChangeOeration
{
    public string text { get; set; } = "";
    public string rangeOffset { get; set; } = "";
    public string rangeLength { get; set; } = "";
    public string RangeStart_Line { get; set; } = "";
    public string RangeStart_Character { get; set; } = "";
    public string RangeEnd_Line { get; set; } = "";
    public string RangeEnd_Character { get; set; } = "";
}

public class GitChange
{
    public int added { get; set; }
    public int deleted { get; set; }
    public int modified { get; set; }
}

public class Changes
{
    public int charactersAdded { get; set; }
    public int charactersDeleted { get; set; }
    public int AddOperations { get; set; }
    public int DeleteOperations { get; set; }
}

public class ProjectInfo
{
    public Dictionary<string, int> LOCPerLanguage { get; set; } = new Dictionary<string, int>();
    public int LOC { get; set; }
}

public class Event
{
    public EventName EventName { get; set; }
    public long EventTime { get; set; }
    public string SessionId { get; set; } = "";
    public string MachineId { get; set; } = "";
    public Uri? DocumentUri { get; set; }
    public int? documentId { get; set; }
    public Operation Operation { get; set; }
    public ChangeOeration? ChangeOperation { get; set; }
    public Changes? Changes { get; set; }
    public GitChange? GitChange { get; set; }
    public ProjectInfo? ProjectInfo { get; set; }

    public Event() { }

    public Event(EventName eventName)
    {
        EventName = eventName;
    }


    public override string ToString()
    {
        return $"EventName: {EventName}, EventTime: {EventTime}, SessionId: {SessionId}, MachineId: {MachineId}, DocumentUri: {DocumentUri}, documentId: {documentId}, Operation: {Operation}, ChangeOperation: {ChangeOperation}, Changes: {Changes}, GitChange: {GitChange}, ProjectInfo: {ProjectInfo}";
    }
}
