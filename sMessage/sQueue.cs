using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sMessage.Queue
{
    // 제네릭 타입 T를 사용하는 sMessageQueue 클래스 정의
    class sQueue<T>
    {
        // 메시지가 추가될 때 발생하는 동기 이벤트
        public event EventHandler<MessageEventArgs<T>> MessageAddedSync;
        // 메시지가 추가될 때 발생하는 비동기 이벤트
        public event AsyncEventHandler<MessageEventArgs<T>> MessageAddedAsync;

        // 스레드 안전을 위한 락 객체들
        private readonly object _msgLock = new object();
        private readonly object _queueSizeLock = new object();
        // 메시지를 저장하는 리스트
        private readonly List<T> _msg = new List<T>();
        // 큐의 최대 크기
        private int _queueSize = 10;

        // 큐의 이름 (기본값: "MyQueue")
        public string Name { get; set; } = "MyQueue";

        // 메시지 프로퍼티 - 큐에서 메시지를 가져오거나 추가
        public T Message
        {
            get
            {
                // 디버그 모드일 때 로그 출력
#if DEBUG
                Console.WriteLine("[DEBUG] " + Name + " >> Get Queue Message");
#endif

                T ret = default(T);
                lock (_msgLock)
                {
                    if (_msg.Count > 0)
                    {
                        ret = _msg[0];
                        _msg.RemoveAt(0);
                    }
                }

                return ret;
            }
            set
            {
                // 디버그 모드일 때 로그 출력
#if DEBUG
                Console.WriteLine("[DEBUG] " + Name + " >> Set Queue Message");
#endif

                lock (_msgLock)
                {
                    _msg.Add(value);

                    // 큐 크기가 설정된 크기를 초과하면 가장 오래된 메시지 제거
                    if (_msg.Count > Size)
                    {
                        _msg.RemoveAt(0);
                    }

                    // 메시지 추가 이벤트 발생
                    OnMessageAdded(new MessageEventArgs<T>(value));
                }
            }
        }

        // 큐의 최대 크기 설정 및 조회
        public int Size
        {
            set
            {
                lock (_queueSizeLock)
                {
                    _queueSize = value;
                }
            }
            get
            {
                lock (_queueSizeLock)
                {
                    return _queueSize;
                }
            }
        }

        // 큐에 있는 메시지의 수 조회
        public int Count
        {
            get
            {
                lock (_msgLock)
                {
                    return _msg.Count;
                }
            }
        }

        // 큐에 메시지가 있는지 확인
        public bool GetReady
        {
            get
            {
                lock (_msgLock)
                {
                    return _msg.Count > 0;
                }
            }
        }

        // 동기 이벤트 처리 메서드
        protected virtual void OnMessageAdded(MessageEventArgs<T> e)
        {
            #if DEBUG
                Console.WriteLine("[DEBUG] " + Name + " >> Event OnMessageAdded");
            #endif
            MessageAddedSync?.Invoke(this, e);
            OnMessageAddedAsync(e);
        }

        // 비동기 이벤트 처리 메서드
        protected virtual async void OnMessageAddedAsync(MessageEventArgs<T> e)
        {
            if (MessageAddedAsync != null)
            {
                var delegates = MessageAddedAsync.GetInvocationList();
                List<Task> tasks = new List<Task>();

                foreach (var del in delegates)
                {
                    if (del is AsyncEventHandler<MessageEventArgs<T>> asyncHandler)
                    {
                        tasks.Add(asyncHandler.Invoke(this, e));
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }

    // 비동기 이벤트 핸들러를 위한 대리자 정의
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);

    // 메시지 이벤트 인자 클래스 정의
    public class MessageEventArgs<T> : EventArgs
    {
        public T Message { get; }

        public MessageEventArgs(T message)
        {
            Message = message;
        }
    }
}
