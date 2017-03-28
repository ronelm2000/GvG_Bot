using Discord;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Discord.Commands;
using System.Collections.Concurrent;

namespace GvG_Core_Bot.Main
{
    public class GvG_GameService : IGameService, IDisposable
    {
        IDependencyMap _map;
        public ConcurrentDictionary<ulong, GvGGame> ListOfGames { get; set; } = new ConcurrentDictionary<ulong, GvGGame>();

        public GvG_GameService (IDependencyMap map)
        {
            _map = map;
        }

        public GvGGame GetServerInstance(IGuild server)
        {
            if (ListOfGames.ContainsKey(server.Id))
            {
                return ListOfGames[server.Id];
            } else
            {
                var newGameInstance = new GvGGame(_map, server);
                ListOfGames.AddOrUpdate(server.Id, newGameInstance, (x,former)=>newGameInstance);
                return newGameInstance;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GvG_GameService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public interface IGameService
    {
        ConcurrentDictionary<ulong, GvGGame> ListOfGames { get; set; }
    }
}
