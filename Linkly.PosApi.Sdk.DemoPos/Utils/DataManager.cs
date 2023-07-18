using Linkly.PosApi.Sdk.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linkly.PosApi.Sdk.DemoPos.Utils;

internal class DataManager : IDisposable
{
    internal SessionData Sessions { get; set; } = new SessionData();
    internal Lane? Current { get; private set; }
    internal bool LastSessionInterrupted { get ; set; }
    internal Guid? LastInterruptedSessionId { get; set; }

    private const string Filename = "sessions.json";
    private const string TimestampFormat = "MM/dd/yyyy hh:mm:ss.fff tt";

    private readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    internal void SaveCurrent(Lane lane)
    {
        if (lane == null)
            return;

        Sessions.Lanes.Except(new[] { lane }).ToList().ForEach(x => x.LastActive = false);
        lane.LastActive = true;
        Current = lane;
        Save();
    }

    internal void Load()
    {
        try
        {
            string contents = string.Empty;

            if (File.Exists(Filename))
                contents = File.ReadAllText(Filename);

            if (contents?.Length > 0)
            {
                var session = JsonSerializer.Deserialize<SessionData>(contents, _options);
                if (session is not null)
                    Sessions = session;

                if (Sessions.Lanes.Count == 0)
                    Sessions.Lanes.Add(new Lane());
            }
            else
                Sessions.Lanes.Add(new Lane());

            var current = Sessions.Lanes.Where(x => x.LastActive)?.FirstOrDefault();
            if (current == default || current is null)
                current = Sessions.Lanes.FirstOrDefault();

            Current = current;
            var lastTransaction = Current?.Transactions?.OrderBy(t => t.RequestTimestamp).LastOrDefault();
            if(lastTransaction != null
                && lastTransaction.Response == null && lastTransaction.Error == null)
            {
                LastSessionInterrupted = true;
                LastInterruptedSessionId = lastTransaction.SessionId;
            }
        }
        catch
        { 
            // happening in the background and not user-facing
        }
    }

    internal void SaveTransaction<T>(Guid sessionId, string? type = null, T? request = default, T? response = default, ErrorResponse? error = null)
    {
        if (Current is not null)
        {
            var item = Current.Transactions?.Where(x => x.SessionId.Equals(sessionId))?.FirstOrDefault();
            if (item is null)
            {
                Current.Transactions!.Add(new() { SessionId = sessionId, Request = request, Response = response, Error = error, Type = type, RequestTimestamp = DateTime.Now.ToString(TimestampFormat) });
            }
            else
            {
                item.ResponseTimestamp = DateTime.Now.ToString(TimestampFormat);
                if (response is not null)
                    item.Response = response;
                if (error is not null)
                    item.Error = error;
            }

            Save();
        }
    }

    internal void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Sessions);
            File.WriteAllText(Filename, json);
        }
        catch 
        {
            // happening in the background and not user-facing
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Save();
    }
}