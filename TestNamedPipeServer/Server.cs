using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static sCommunication.NamePipe.sNamePipe;

namespace TestNamedPipeServer
{
    public partial class Server : Form
    {
        /// <summary>
        /// 서버가 클라이언트의 연결을 기다리고 데이터를 송수신하는 메서드입니다.
        /// </summary>
        /// <param name="pipeName">네임드 파이프의 이름입니다.</param>
        private static void RunServer(string pipeName)
        {
            // 연결 시도 횟수를 추적하는 변수입니다.
            int waitCount = 0;
            // 설정된 타임아웃 간격만큼 최대 연결 시도 횟수를 반복합니다.
            while (waitCount < TimeoutDurations.Length)
            {
                try
                {
                    // 네임드 파이프 서버 스트림을 생성합니다.
                    using (NamedPipeServerStream serverStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, MaxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        Console.WriteLine($"Server waiting for client connections... Wait for {TimeoutDurations[waitCount] / 1000} seconds");

                        // 클라이언트의 연결을 비동기적으로 기다립니다.
                        if (Task.Run(() => serverStream.WaitForConnectionAsync()).Wait(TimeoutDurations[waitCount]))
                        {
                            Console.WriteLine("Server connected to a client.");
                            // 연결 성공 시, 갱신 시도를 초기화합니다.
                            waitCount = 0;
                        }
                        else
                        {
                            // 연결 실패 시, 로그를 출력하고 다음 시도로 넘어갑니다.
                            Console.WriteLine("Failed to connect.");
                            waitCount++;
                            continue;
                        }

                        // 클라이언트와의 연결이 유지되는 동안 데이터 송수신을 반복합니다.
                        try
                        {
                            while (true)
                            {
                                // 클라이언트로부터 데이터를 수신합니다.
                                string receivedMessage = ReceiveData(serverStream);
                                Console.WriteLine($"Server received message from client: {receivedMessage}");

                                // 클라이언트에 응답 메시지를 전송합니다.
                                string responseMessage = $"Hello from server to client {receivedMessage.Split(' ').Last()}";
                                SendData(serverStream, responseMessage);

                                Console.WriteLine("Server sent response to client.");

                                // 짧은 대기 시간을 둡니다.
                                Thread.Sleep(10);
                            }
                        }
                        catch (IOException)
                        {
                            // 연결이 끊어진 경우, 로그를 출력하고 다음 연결을 기다립니다.
                            Console.WriteLine("Connection lost with client.");
                            continue;
                        }
                        catch (Exception e)
                        {
                            // 통신 중 발생하는 기타 예외를 처리합니다.
                            Console.WriteLine($"Error during communication: {e.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 서버 설정 중 발생하는 예외를 처리합니다.
                    Console.WriteLine($"Server error: {ex.Message}");
                    waitCount++;
                }
            }

            // 모든 연결 시도가 실패했을 경우, 로그를 출력합니다.
            if (waitCount >= TimeoutDurations.Length)
            {
                Console.WriteLine("Failed to establish connection after multiple attempts.");
            }
        }

        public Server()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 사용자 인터페이스에서 "Start Server" 버튼을 클릭할 때 호출되는 이벤트 핸들러입니다.
        /// 서버를 실행하는 백그라운드 작업을 시작합니다.
        /// </summary>
        /// <param name="sender">이벤트 발생 객체입니다.</param>
        /// <param name="e">이벤트 데이터입니다.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            // RunServer 메서드를 별도의 작업(Task)으로 실행합니다.
            Task serverTask = Task.Run(() => RunServer(PipeName));
        }
    }
}
