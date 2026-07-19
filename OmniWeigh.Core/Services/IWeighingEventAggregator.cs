using System;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IWeighingEventAggregator
    {
        event EventHandler<WeighingHistoryItemDto>? WeighingCreated;
        void PublishWeighingCreated(WeighingHistoryItemDto item);
    }
}
