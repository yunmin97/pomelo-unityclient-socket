/*
 * @Author: yunmin
 * @Email: 362279869@qq.com
 * @Date: 2020-02-24 20:10:36
 */

using System;
using SimpleJson;
using UnityEngine;
using Pomelo.DotNetClient;
using Battlehub.Dispatcher;

namespace Pomelo.Extend
{
    /// <summary>
    /// connect to a pomelo server
    /// </summary> 
    public class Connector : MonoBehaviour
    {
        // should this connector be marked as DontDestroyOnLoad
        public bool Persist = false;

        // network changed event
        public event Action<NetWorkState> NetState;

        // net connect proxy
        PomeloClient m_Conn;

        /// <summary>
        /// use this for initialization
        /// </summary>
        private void Start()
        {
            if (Persist)
            {
                DontDestroyOnLoad(gameObject);
            }
            if (FindObjectOfType<Dispatcher>() == null)
            {
                gameObject.AddComponent<Dispatcher>();
            }
        }

        /// <summary>
        /// use this for clear work
        /// </summary>
        private void OnDestroy()
        {
            Disconnect();
        }

        /// <summary>
        /// connect to server and response
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="user">handshake msg for connect</param> 
        /// <param name="action">default request callback</param>
        public void Connect(string ip, int port, JsonObject user, Action<JsonObject> action)
        {
            if (m_Conn != null)
            {
                Debug.Log("the network has been connected!");
                return;
            }
            Debug.Log("connect to ip: " + ip + " port: " + port);
            m_Conn = new PomeloClient();
            m_Conn.NetWorkStateChangedEvent += (state) =>
            {
                Debug.Log("network state: " + state);
                if (NetState != null)
                {
                    NetState.Invoke(state);
                }
            };
            m_Conn.initClient(ip, port, () =>
            {
                m_Conn.connect(user, (rt) =>
                {
                    Dispatcher.Current.BeginInvoke(() =>
                    {
                        action.Invoke(rt);
                    });
                });
            });
        }

        /// <summary>
        /// disconnect the client
        /// </summary>
        public void Disconnect()
        {  
            if (m_Conn != null)
            {
                m_Conn.disconnect();
                m_Conn = null;
            }
            NetState = null;
        }

        /// <summary>
        /// add event listener, process broadcast message
        /// </summary>
        /// <param name="eventName">eg: onReceive</param>
        /// <param name="action"></param>
        public void On(string eventName, Action<JsonObject> action)
        {
            m_Conn.on(eventName, (result) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    action.Invoke(result);
                });
            });
        }

        /// <summary>
        /// request server with a custom data and response
        /// </summary>
        /// <param name="route"></param>
        /// <param name="data"></param>
        /// <param name="action"></param>
        public void Request(string route, JsonObject data, Action<JsonObject> action)
        {
            m_Conn.request(route, data, (result) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    action.Invoke(result);
                });
            });
        }

        /// <summary>
        /// request server and response
        /// </summary>
        /// <param name="route"></param>
        /// <param name="action"></param>
        public void Request(string route, Action<JsonObject> action)
        {
            m_Conn.request(route, (result) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    action.Invoke(result);
                });
            });
        }

        /// <summary>
        /// notify server without response
        /// </summary>
        /// <param name="route"></param>
        /// <param name="data"></param>
        public void Notify(string route, JsonObject data)
        {
            m_Conn.notify(route, data);
        }
    }
}

