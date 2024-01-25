using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// 네임스페이스 sSocketHelper 선언.
namespace sSocketHelper
{
    // cSocketHelper라는 정적 클래스를 선언합니다. 이 클래스는 소켓 통신과 관련된 공통 유틸리티 함수들을 제공합니다.
    public static class cSocketHelper
    {
        /// <summary>
        /// 주어진 문자열을 IP 주소로 파싱하는 메서드.<br></br>
        /// 올바른 IP 주소 형식인 경우 IPAddress 객체를 반환하고, 그렇지 않으면 예외를 발생시킵니다.
        /// </summary>
        /// <param name="ipAddress">파싱할 IP 주소 문자열입니다.</param>
        /// <returns>파싱된 IPAddress 객체입니다.</returns>
        /// <exception cref="FormatException">잘못된 IP 주소 형식일 경우 발생합니다.</exception>
        public static IPAddress ParseIPAddress(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out IPAddress address))
            {
                return address;
            }
            else
            {
                throw new FormatException("Invalid IP address format.");
            }
        }

        /// <summary>
        /// 주어진 문자열을 포트 번호로 파싱하는 메서드.<br></br>
        /// 올바른 포트 형식인 경우 정수로 변환하여 반환하고, 그렇지 않으면 예외를 발생시킵니다.
        /// </summary>
        /// <param name="portString">파싱할 포트 번호 문자열입니다.</param>
        /// <returns>파싱된 포트 번호를 정수로 반환합니다.</returns>
        /// <exception cref="FormatException">잘못된 포트 형식일 경우 발생합니다.</exception>
        public static int ParsePort(string portString)
        {
            if (int.TryParse(portString, out int port))
            {
                return port;
            }
            else
            {
                throw new FormatException("Invalid port format.");
            }
        }

        /// <summary>
        /// 텍스트를 UI 컨트롤에 안전하게 추가하는 메서드.<br></br>
        /// UI 스레드에서 실행되어야 하는 컨트롤의 변경 사항은 Invoke를 사용하여 스레드 간 문제를 해결합니다.<br></br>
        /// control이 null이 아닌 경우 InvokeRequired 속성을 검사하여 필요시 컨트롤의 스레드에서 실행되도록 합니다.<br></br>
        /// control이 null인 경우 콘솔에 텍스트를 출력합니다.
        /// </summary>
        /// <param name="control">텍스트를 추가할 Control 객체입니다.</param>
        /// <param name="text">Control에 추가할 텍스트입니다.</param>
        public static void AppendText(Control control, string text)
        {
            if (control != null)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new Action<string>(s => AppendText(control, s)), text);
                }
                else
                {
                    Console.WriteLine(text);
                }
            }
            else
            {
                Console.WriteLine(text);
            }
        }

        public static async Task<string> ReceiveDataAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];

            if (stream.DataAvailable)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                return bytesRead > 0 ? Encoding.UTF8.GetString(buffer, 0, bytesRead) : null;
            }
            else
                return null;
        }



        public static async Task SendDataAsync(NetworkStream stream, string data)
        {
            Console.WriteLine($"[cSocketHelper] Send Message {data}");
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
