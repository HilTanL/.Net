﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CLog
{
    /// <summary>
    /// 企业应用框架的日志类
    /// </summary>
    /// <remarks>此日志类提供高性能的日志记录实现。
    /// 当调用Write方法时不会造成线程阻塞,而是立即完成方法调用,因此调用线程不用等待日志写入文件之后才返回。</remarks>
    public class Log : IDisposable
    {
        private static string StrLogPath = Environment.CurrentDirectory + "\\Log\\";
        private static Log logInstance;

        //日志对象的缓存队列
        private static Queue<Msg> msgs;
        //日志文件保存的路径
        private static string path;
        //日志写入线程的控制标记
        private static bool state;
        //日志记录的类型
        private static LogType type;
        //日志文件生命周期的时间标记
        private static DateTime TimeSign;
        //日志文件写入流对象
        private static StreamWriter writer;
        
        /// <summary>
        /// 创建日志对象的新实例，根据指定的日志文件路径和指定的日志文件创建类型
        /// </summary>
        /// <param name="p">日志文件保存路径</param>
        /// <param name="t">日志文件创建方式的枚举</param>
        private Log(string p, LogType t)
        {
            if (msgs == null)
            {
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);

                state = true;
                path = p;
                type = t;
                msgs = new Queue<Msg>();
                Thread thread = new Thread(work);
                thread.Start();
            }
        }

        public static Log GetInstance()
        {
            if(logInstance==null)
            {
                logInstance = new Log(Log.StrLogPath,LogType.Daily);
            }
            return logInstance;
        }
        
        //日志文件写入线程执行的方法
        private void work()
        {
            while (true)
            {
                //判断队列中是否存在待写入的日志
                if (msgs.Count > 0)
                {
                    Msg msg = null;
                    lock (msgs)
                    {
                        msg = msgs.Dequeue();
                    }
                    if (msg != null)
                    {
                        FileWrite(msg);
                    }
                }
                else
                {
                    //判断是否已经发出终止日志并关闭的消息
                    if (state)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        FileClose();
                    }
                }
            }
        }

        //根据日志类型获取日志文件名，并同时创建文件到期的时间标记
        //通过判断文件的到期时间标记将决定是否创建新文件。
        private string GetFilename()
        {
            DateTime now = DateTime.Now;
            string format = "";
            switch (type)
            {
                case LogType.Daily:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddDays(1);
                    format = "yyyyMMdd'.log'";
                    break;
                case LogType.Weekly:
                    TimeSign = new DateTime(now.Year, now.Month, now.Day);
                    TimeSign = TimeSign.AddDays(7);
                    format = "yyyyMMdd'.log'";
                    break;
                case LogType.Monthly:
                    TimeSign = new DateTime(now.Year, now.Month, 1);
                    TimeSign = TimeSign.AddMonths(1);
                    format = "yyyyMM'.log'";
                    break;
                case LogType.Annually:
                    TimeSign = new DateTime(now.Year, 1, 1);
                    TimeSign = TimeSign.AddYears(1);
                    format = "yyyy'.log'";
                    break;
            }
            return now.ToString(format);
        }

        //写入日志文本到文件的方法
        private void FileWrite(Msg msg)
        {
            try
            {
                if (writer == null)
                    FileOpen();
                //判断文件到期标志，如果当前文件到期则关闭当前文件创建新的日志文件
                if (DateTime.Now >= TimeSign)
                {
                    FileClose();
                    FileOpen();
                }
                writer.Write(msg.Datetime);
                writer.Write('\t');
                writer.Write(msg.Type);
                writer.Write('\t');
                writer.WriteLine(msg.Text);
                writer.Flush();

            }
            catch (Exception e)
            {
                Console.Out.Write(e);
            }
        }

        //打开文件准备写入
        private void FileOpen()
        {
            writer = new StreamWriter(path + GetFilename(), true, Encoding.UTF8);
        }

        //关闭打开的日志文件
        private void FileClose()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
                writer.Dispose();
                writer = null;
            }
        }

        /// <summary>
        /// 写入新日志，根据指定的日志对象Msg
        /// </summary>
        /// <param name="msg">日志内容对象</param>
        private void Write(Msg msg)
        {
            Log log = new Log(StrLogPath,LogType.Daily);
            if (msg != null)
            {
                lock (msgs)
                {
                    msgs.Enqueue(msg);
                }
            }
        }

        public void WriteError(params string[] strMsgs)
        {
            Write(MsgType.Error, strMsgs);
        }

        public void WriteWarn(params string[] strMsgs)
        {
            Write(MsgType.Warn, strMsgs);
        }

        public void WriteInfo(params string[] strMsgs)
        {
            Write(MsgType.Info, strMsgs);
        }

        /// <summary>
        /// 写入新日志，根据指定的日志内容和信息类型，采用当前时间为日志时间写入新日志
        /// </summary>
        /// <param name="text">日志内容</param>
        /// <param name="type">信息类型</param>
        public void Write(MsgType type,params string[] strMsgs)
        {
            string strMsgText = "";

            int i = 0;
            foreach (string strMsg in strMsgs)
            {
                if (i == 0)
                    strMsgText = strMsg;
                else
                    strMsgText += "-" + strMsg;
                ++i;
            }

            Write(new Msg(strMsgText, type));
        }


        /// <summary>
        /// 销毁日志对象
        /// </summary>
        public void Dispose()
        {
            state = false;
        }

    }
}