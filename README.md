# Customized C# Code, shared project
README.md는 ChatGPT를 통해 작성되었습니다.

# Generic

## 1. Delegate

# Customized UI

## 1. Map Control

# Communication

## 1. NamedPipe

### Overview
이 프로젝트는 네임드 파이프 통신을 위한 `sNamePipe` 헬퍼 클래스를 제공합니다. 이 클래스는 서버와 클라이언트 간의 효율적인 통신을 위해 설계되었습니다.

### Features
- **동적 연결**: 서버와 클라이언트는 `PipeName`을 사용하여 동적으로 연결됩니다.
- **다중 인스턴스 지원**: 서버는 동시에 여러 클라이언트 인스턴스를 수용할 수 있습니다 (`MaxNumberOfServerInstances` 참조).
- **메시지 교환**: `SendData`와 `ReceiveData` 메서드를 통해 메시지를 송수신할 수 있습니다.

### Installation
네임드 파이프 통신을 위한 별도의 설치 과정은 필요하지 않습니다. 소스 파일(`sNamedPipe.cs`)을 프로젝트에 포함시키기만 하면 됩니다.

### Usage
#### 메시지 송신
```csharp
PipeStream stream; // 파이프 스트림 인스턴스
string message = "Hello, World!";
sNamePipe.SendData(stream, message);
```

#### 메시지 수신
```csharp
PipeStream stream; // 파이프 스트림 인스턴스
string receivedMessage = sNamePipe.ReceiveData(stream);
```

## 2. TCP Socket Communication Project

### Overview
이 프로젝트는 TCP 소켓 통신을 관리하기 위한 서버와 클라이언트 매니저를 제공합니다. `cSocketServerManager`와 `cSocketClientManager` 클래스를 사용하여 네트워크 기반의 어플리케이션을 쉽게 구축할 수 있습니다.

### Features
- **cSocketServerManager**: TCP 서버를 관리하며 클라이언트 연결 수락 및 데이터 송수신을 담당합니다.
- **cSocketClientManager**: TCP 클라이언트를 관리하며 서버에 연결하고 데이터를 송수신합니다.
- **cSocketManagerBase**: 서버와 클라이언트 매니저에 공통적인 기능을 제공합니다.

### Installation
프로젝트는 .NET Framework 또는 .NET Core 환경에서 작동하도록 설계되었습니다. 이 코드를 사용하기 위해 특별한 설치 과정은 필요하지 않습니다. 기존의 .NET 프로젝트에 클래스 파일들을 추가하기만 하면 됩니다.

### Usage
#### Server
```csharp
var serverManager = new cSocketServerManager("127.0.0.1", 8080);
serverManager.StartServer();
```

#### Client
```csharp
var clientManager = new cSocketClientManager("127.0.0.1", 8080);
clientManager.StartClient();
```
