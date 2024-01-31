# sQueue<T> Class for C#

## 개요
`sQueue<T>`는 C#에서 사용자 정의 제네릭 메시지 큐를 구현한 클래스입니다. 이 큐는 동기 및 비동기 이벤트 처리, 스레드 안전성, 큐 크기 관리 등의 기능을 제공합니다. 표준 `Queue<T>` 클래스와 비교하여 확장성과 유연성이 강화된 큐 구현입니다.

## 특징
- **이벤트 기반의 메시지 처리**: 메시지가 추가될 때 동기 및 비동기 이벤트를 발생시킵니다.
- **스레드 안전성**: 락(lock)을 사용하여 다중 스레드 환경에서의 안전한 접근을 보장합니다.
- **동적 큐 크기 관리**: 설정된 최대 크기를 초과하면 오래된 메시지를 자동으로 제거합니다.
- **디버그 모드 로깅**: 디버그 모드에서 큐의 동작을 로깅하여 문제 해결에 도움을 줍니다.

## 사용 방법

### 클래스 구현
이 라이브러리에는 다음 클래스들이 포함되어 있습니다:
- `sQueue<T>`: 메시지 큐의 메인 클래스.
- `MessageEventArgs<T>`: 메시지 이벤트에 대한 정보를 제공하는 클래스.
- `AsyncEventHandler<TEventArgs>`: 비동기 이벤트를 처리하기 위한 대리자(delegate).

### 예제 코드
```csharp
var messageQueue = new sQueue<string>();
messageQueue.MessageAddedSync += (sender, e) => 
{
    Console.WriteLine("메시지 동기적으로 추가됨: " + e.Message);
};
messageQueue.MessageAddedAsync += async (sender, e) => 
{
    await Task.Delay(1000); // 비동기 처리 예시
    Console.WriteLine("메시지 비동기적으로 추가됨: " + e.Message);
};

messageQueue.Message = "새 메시지";
```

## 장단점
### 장점
- 확장성과 유연성이 뛰어난 이벤트 기반 구조.
- 스레드 안전성 제공.
- 자동 크기 관리로 메모리 사용 최적화.
- 디버그 모드에서 유용한 로깅 기능.
### 단점
- 구현의 복잡성.
- 스레드 동기화에 따른 성능 오버헤드 가능성.
- 표준 라이브러리가 아닌 점을 고려한 호환성 및 유지보수 필요.

## 라이선스
이 프로젝트는 MIT 라이선스 하에 배포됩니다.