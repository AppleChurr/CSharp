using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace sCommunication.NamePipe
{
    /// <summary>
    /// PipeHelper 클래스는 네임드 파이프 통신을 위한 헬퍼 메서드를 제공합니다.
    /// </summary>
    public static class sNamePipe
    {

        /// <summary>
        /// 네임드 파이프의 이름을 정의합니다. 서버와 클라이언트가 이 이름을 사용하여 연결합니다.
        /// </summary>
        public const string PipeName = "PipeServer";

        /// <summary>
        /// 서버에서 동시에 수용할 수 있는 최대 클라이언트 인스턴스 수를 정의합니다.
        /// </summary>
        public const int MaxNumberOfServerInstances = 10;

        /// <summary>
        /// 서버 연결 시도 시 사용할 타임아웃 간격을 밀리초 단위로 정의합니다.
        /// 연결 시도 간 간격은 점점 증가합니다.
        /// </summary>
        public static readonly int[] TimeoutDurations = { 1000, 2000, 4000, 8000, 16000 };

        /// <summary>
        /// 주어진 파이프 스트림을 통해 메시지를 보냅니다.
        /// </summary>
        /// <param name="stream">메시지를 보낼 파이프 스트림입니다.</param>
        /// <param name="message">보낼 메시지입니다.</param>
        public static void SendData(PipeStream stream, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 주어진 파이프 스트림에서 메시지를 수신합니다.
        /// </summary>
        /// <param name="stream">메시지를 수신할 파이프 스트림입니다.</param>
        /// <returns>수신된 메시지를 반환합니다.</returns>
        public static string ReceiveData(PipeStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}