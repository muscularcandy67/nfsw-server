﻿using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OfflineServer.Servers.Xmpp
{
    public abstract class XmppServer
    {
        public Int32 port;
        public String jidPrepender;
        protected TcpListener listener;
        protected TcpClient client;
        protected NetworkStream stream;
        protected SslStream sslStream;
        protected Int32 personaId;
        protected X509Certificate certificate;
        protected Decoder decoder = Encoding.UTF8.GetDecoder();
        protected CancellationTokenSource cts;
        protected CancellationToken ct;
        protected Boolean isSsl;

        public async Task<String> read(Boolean forceNoSsl = false)
        {
            byte[] data = new byte[client.ReceiveBufferSize];
            int bytesRead;
            string request;
            if (isSsl & !forceNoSsl)
            {
                var readTask = await sslStream.ReadAsync(data, 0, Convert.ToInt32(client.ReceiveBufferSize), ct).ConfigureAwait(false);
                bytesRead = readTask;
                char[] chars = new char[decoder.GetCharCount(data, 0, bytesRead)];
                decoder.GetChars(data, 0, bytesRead, chars, 0);
                request = new string(chars);
            }
            else
            {
                var readTask = await stream.ReadAsync(data, 0, Convert.ToInt32(client.ReceiveBufferSize), ct).ConfigureAwait(false);
                bytesRead = readTask;
                request = Encoding.UTF8.GetString(data, 0, bytesRead);
            }
            if (!String.IsNullOrWhiteSpace(request)) ExtraFunctions.log(String.Format("Acknowledged xmpp packet {0}.", request), "XmppServer");
            return request;
        }

        public async Task write(String message, Boolean forceNoSsl = false)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            if (isSsl & !forceNoSsl)
            {
                await sslStream.WriteAsync(msg, 0, msg.Length, ct).ConfigureAwait(false);
                await sslStream.FlushAsync().ConfigureAwait(false);
            }
            else
            {
                await stream.WriteAsync(msg, 0, msg.Length, ct).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
            ExtraFunctions.log(String.Format("Sent xmpp packet {0}.", message), "XmppServer");
        }

        public abstract void initialize();
        public abstract void doHandshake();
        public abstract void doLogin(Int32 newPersonaId);
        public abstract void doLogout(Int32 personaId);
        public abstract void listenLoop();
        public abstract void shutdown();
    }
}