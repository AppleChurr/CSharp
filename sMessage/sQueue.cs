using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sMessage.Queue
{
    // T 타입의 제네릭 메시지를 처리하는 사용자 정의 큐 클래스
    class sQueue<T>
    {
        // 메시지가 큐에 추가될 때 발생하는 동기 이벤트
        public event EventHandler<MessageEventArgs<T>> MessageAddedSync;

        // 메시지가 큐에 추가될 때 발생하는 비동기 이벤트
        public event AsyncEventHandler<MessageEventArgs<T>> MessageAddedAsync;

        // 스레드 안전성을 보장하기 위한 락 객체
        private readonly object _msgLock = new object();
        private readonly object _queueSizeLock = new object();

        // 실제 메시지를 저장하는 큐
        private readonly Queue<T> _msg = new Queue<T>();

        // 큐의 최대 크기 (기본값: 10)
        private int _queueSize = 10;

        // 큐의 이름 (기본값: "MyQueue")
        public string Name { get; set; } = "MyQueue";

        // 큐에서 메시지를 가져오거나 추가하는 프로퍼티
        public T Message
        {
            get
            {
                T ret = default(T);
                lock (_msgLock)
                {
                    if (_msg.Count > 0)
                    {
                        // 큐의 첫 번째 메시지를 반환하고 큐에서 제거
                        ret = _msg.Dequeue();
                    }
                }

                // 디버그 모드에서 로그 출력
#if DEBUG
                Console.WriteLine($"[DEBUG] {Name} >> Dequeue Message {Count}/{Size}");
#endif

                return ret;
            }
            set
            {
                lock (_msgLock)
                {
                    // 메시지를 큐에 추가
                    _msg.Enqueue(value);

                    // 큐의 크기가 최대 크기를 초과하는 경우, 가장 오래된 메시지 제거
                    if (_msg.Count > Size)
                    {
#if DEBUG
                        Console.WriteLine($"[DEBUG] {Name} >> Queue is full...");
#endif
                        _msg.Dequeue();
                    }

                    // 메시지 추가 이벤트 발생
                    OnMessageAdded(new MessageEventArgs<T>(value));

                    // 디버그 모드에서 로그 출력
#if DEBUG
                    Console.WriteLine($"[DEBUG] {Name} >> Enqueue Message {Count}/{Size}");
#endif
                }
            }
        }

        // 큐의 최대 크기를 설정하고 조회하는 프로퍼티
        public int Size
        {
            set
            {
                lock (_queueSizeLock)
                {
                    // 디버그 모드에서 크기 변경 로그 출력
#if DEBUG
                    Console.WriteLine($"[DEBUG] {Name} >> Size Change ( {_queueSize} -> {value} )");
#endif

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

        // 현재 큐에 있는 메시지의 수를 조회하는 프로퍼티
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

        // 큐에 메시지가 있는지 확인하는 프로퍼티
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

        // 큐의 모든 메시지를 제거하는 메서드
        public void Clear()
        {
            lock (_msgLock)
            {
                _msg.Clear();
            }
        }

        // 메시지가 추가될 때 호출되는 동기 이벤트 처리 메서드
        protected virtual void OnMessageAdded(MessageEventArgs<T> e)
        {
#if DEBUG
            Console.WriteLine($"[DEBUG] {Name} >> Event OnMessageAdded");
#endif

            MessageAddedSync?.Invoke(this, e);
            OnMessageAddedAsync(e);
        }

        // 메시지가 추가될 때 호출되는 비동기 이벤트 처리 메서드
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

                // 모든 비동기 이벤트 처리를 기다림
                await Task.WhenAll(tasks);
            }
        }
    }

    // 비동기 이벤트를 위한 대리자 (delegate) 정의
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);

    // 메시지 이벤트 인자 클래스 정의
    public class MessageEventArgs<T> : EventArgs
    {
        // 이벤트에 포함될 메시지
        public T Message { get; }

        public MessageEventArgs(T message)
        {
            Message = message;
        }
    }
}
