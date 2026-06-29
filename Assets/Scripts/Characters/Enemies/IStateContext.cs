using System;

public interface IStateContext
{
    void Execute<CommandType>(CommandType command) where CommandType : ICommand;
    ResultType ExecuteQuery<ResultType>(ICommandQuery<ResultType> query);

    void SetData<DataType>(StateDataKey<DataType> key, DataType data);
    DataType GetData<DataType>(StateDataKey<DataType> key);
    bool TryGetData<DataType>(StateDataKey<DataType> key, out DataType data);

    event Action<ICommand> OnCommandExecuted;
}
