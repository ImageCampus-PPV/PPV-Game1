using System;

public interface IStateContext
{
    void Execute<CommandType>(CommandType command) where CommandType : ICommand;
    ResultType ExecuteQuery<ResultType>(ICommandQuery<ResultType> query);

    event Action<ICommand> OnCommandExecuted;
}
