using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace IdentityServer.SqlInterceptors;

public interface ICustomInterceptor : IInterceptor;

public class CustomCommandInterceptor(ILogger<CustomCommandInterceptor> _logger) : DbCommandInterceptor, ICustomInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        _logger.LogInformation("Command Text: {CommandText}", command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }
}