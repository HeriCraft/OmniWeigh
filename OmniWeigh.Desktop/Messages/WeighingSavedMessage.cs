using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Desktop.Messages
{
    public class WeighingSavedMessage
    {
        public HistoryRecordDto NewRecord { get; }

        public WeighingSavedMessage(HistoryRecordDto record)
        {
            NewRecord = record;
        }
    }
}
