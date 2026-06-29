public interface ICommandHandler<CommandType> where CommandType : ICommand
{
    void Execute(CommandType command, IStateContext context);
}
