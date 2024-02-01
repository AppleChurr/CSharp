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

namespace TestNamedPipeClient
{
    public partial class Client : Form
    {
        /// <summary>
        /// 클라이언트가 서버에 연결을 시도하고 데이터를 송수신하는 메서드입니다.
        /// </summary>
        /// <param name="clientId">클라이언트의 ID입니다.</param>
        private static void RunClient(int clientId)
        {
            // 연결 시도 횟수를 추적하는 변수입니다.
            int tryCount = 0;
            // 설정된 타임아웃 간격만큼 최대 연결 시도 횟수를 반복합니다.
            while (tryCount < TimeoutDurations.Length)
            {
                try
                {
                    // 네임드 파이프 클라이언트 스트림을 생성합니다.
                    using (NamedPipeClientStream clientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.None))
                    {
                        Console.Write($"Client attempting to connect to server... Attempt: {tryCount + 1}");
                        // 타임아웃 간격을 사용하여 서버에 연결을 시도합니다.
                        clientStream.Connect(TimeoutDurations[tryCount]);

                        if (clientStream.IsConnected)
                        {
                            Console.WriteLine("Client connected to server.");
                            // 연결 성공 시, 횟수를 초기화 합니다.
                            tryCount = 0;
                        }
       
                        // 연결이 유지되는 동안 반복하여 데이터를 송수신합니다.
                        while (true)
                        {
                            try
                            {
                                // 서버에 메시지를 전송합니다.
                                string message = $"Ping from client at {DateTime.Now} {clientId}";
                                SendData(clientStream, message);

                                // 서버로부터 메시지를 수신합니다.
                                string receivedMessage = ReceiveData(clientStream);
                                Console.WriteLine($"Client received message from server: {receivedMessage}");

                                // 일정 시간 대기합니다.
                                Thread.Sleep(1000);
                            }
                            catch (IOException)
                            {
                                // 연결이 끊어진 경우, 연결 재시도를 위해 반복문을 빠져나갑니다.
                                Console.WriteLine("Connection lost. Attempting to reconnect...");
                                break;
                            }
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // 연결 시도가 타임아웃될 경우, 콘솔에 메시지를 출력합니다.
                    Console.WriteLine($"Connection timeout. Waiting for {TimeoutDurations[tryCount] / 1000} seconds before retrying...");
                }
                catch (Exception ex)
                {
                    // 기타 예외 발생 시, 콘솔에 에러 메시지를 출력합니다.
                    Console.WriteLine($"Error in RunClient: {ex.Message}");
                }
                tryCount += 1;
            }

            // 모든 연결 시도가 실패했을 경우, 콘솔에 메시지를 출력합니다.
            if (tryCount >= TimeoutDurations.Length)
            {
                Console.WriteLine("Failed to connect to server after multiple attempts.");
            }
        }

        public Client()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 사용자 인터페이스에서 "Connect" 버튼을 클릭할 때 호출되는 이벤트 핸들러입니다.
        /// 클라이언트를 실행하는 백그라운드 작업을 시작합니다.
        /// </summary>
        /// <param name="sender">이벤트 발생 객체입니다.</param>
        /// <param name="e">이벤트 데이터입니다.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            // RunClient 메서드를 별도의 작업(Task)으로 실행합니다.
            Task serverTask = Task.Run(() => RunClient((int)numericUpDown1.Value));
        }
    }
}
