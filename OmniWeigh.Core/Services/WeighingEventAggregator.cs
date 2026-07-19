using System;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public class WeighingEventAggregator : IWeighingEventAggregator
    {
        public event EventHandler<WeighingHistoryItemDto>? WeighingCreated;

        public void PublishWeighingCreated(WeighingHistoryItemDto item)
        {
            WeighingCreated?.Invoke(this, item);
        }
    }
}
