using System.Threading;
using System.Threading.Tasks;

namespace Abstractions;

public interface IOutput
{
    Task ProcessAsync(ProcessorResult processorResult, CancellationToken cancellationToken);
}
