Эта библиотека поможет вам удобно связывать сервер с клиентом по TCP протоколу.

Основной идеей заключается создание "мостов" между сервером и клиентом для общения.
Мост - объект, через который вы отправляете данные на сервер и ожидаете ответа или эвента.
(Исключительно инструмент клиента)

Ещё библиотека позволит вам создать свой сервер и задать обработчики пакетов. Всего в пару строчек кода вы можете создать полноценное клиент-серверное приложение. 
Примечание: 
  На данный момент создан только один тип сервера, поддерживающий исключительно одного клиента.
  Сервер принимает клиента, а после - данные от него.
  При разрыве соединения, сервер снова ожидает подключения от клиента. (Вы запросто сможете реализовать переподключвение клиента, используя соответствующий эвент)

Создадим простое клиент-серверное приложение.

Для начала лучше всего (в новом проекте)[optional] создать пару пакетов, которые мы будет отправлять.
Создадим класс, в котором будет храниться сообщение серверу.
P.s. Для сериализации/десериализации данных я использовал библиотеку моего товарища, мы вместе разрабатывали ее и не стесняемся использовать.
Т.к. наш класс не имеет Generic-параметров, то мы может использовать атрибут [PreGenerate] и нам можно не думать о том, чтобы сериализатор смог его успешно с ним работать.
Позже покажу, как заставить сериализатор обработать пакет с Generic-параметрами.
```
using DiNet.InstantTcp.Core;
using ASiNet.Data.Serialization.Attributes;

[PreGenerate]
public class MessagePackage : InstantPackageBase
{
    public string? Nickname { get; set; }
    public string? Text { get; set; }
}
```

Создадим сервер

Его основой являются обработчики пакетов. 
Здесь мы при получении пакета MessagePackage отправим в качестве ответа значение string с перевернутым сообщением.
Учтите: рассчитывается, что клиент будет ожидать ответа сервера InstantResponse<string> с полями [ResponseType ResponseType(Ok/Exception), Guid TargetPackageId, string? Exception, string Value(generic parameter)]
```
server.SetHandler<MessagePackage, string>(x =>
{
    Console.WriteLine($"From {x.Nickname}: {x.Text}");
    return x.Text.Reverse().ToString();
});
```

Реализация сервера
```
//создаем сервер TSoloServer для одного пользователя и зададим ему IP и Port.
var server = new TSoloServer(new("127.0.0.1", 56667)); //"127.0.0.1" - локальный адресс

server.SetHandler<MessagePackage, string>(x =>
{
    Console.WriteLine($"From {x.Nickname}: {x.Text}");
    return x.Text.Reverse().ToString();
});

server.Start(); // запускаем сервер

await server.Updater(new CancellationTokenSource().Token); // начинаем принятие клиентов, получение и обработку данных
```

Создадим клиент

Для начала зарегистрируем тип ответа, чтобы сериализатор распознал пакет, который нам пришел.
```
using DiNet.InstantTcp.Common.Helpers;
SerializerHelper.RegInstantResponseFor<string>(); // так мы сможет получать пакет типа InstantResponse<string>
```

Теперь создадим клиент и подключим к серверу
```
using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Common.Helpers;
using DiNet.InstantTcp.Core;

SerializerHelper.RegInstantResponseFor<string>();

//"127.0.0.1", 56665
var client = new TClient(new() { BridgeCapacity = 10, PackagePollDelay = 30 });

while(!await client.Connect("127.0.0.1", 56667))
    client.Disconnect();

client.AcceptPackages(); //начинаем получать ответы и эвенты от сервера
```

Теперь остановимся на системе мостов.
Есть 3 моста:
EventStackBridge<TEvent> - мост, для получения эвентов от сервера, данные в нем нужно самостоятельно читать по-очереди, вам поможет OnEventAdded, который скажет, что новый эвент, который вам нужно обработать, добавлен.
EventBridge<TEvent> - мост, для получения эвентов от сервера. Данные приходят и отправляются сразу в OnEvent, который попросит вас сразу обработать запрос (читать данные не требуется)
Bridge<TRequest, TResponse>  - стандартный мост для общения сервера и клиента. Вы можете отправлять TRequest, а взамен получать TResponse.

```
var bridge = client.GetInstantBridge<MessagePackage, string>(); мост для получения InstantResponse<string>

bridge.Write(new() { Text = "HELLO WORLD!", Nickname = "Difoldery" }); // отправляет запрос на обработку MessagePackage

var msg = await bridge.Read(); //ожидаем ответ

Console.WriteLine(msg.Value);

Console.ReadLine();
```

Наше приложение готово. 

Есть некоторые ньюансы: 
Если вы создали мост, где получаете пакет типа InstantResponse<string>, то данный ответ будет исключительно для одного моста(Вы не сможете больше создать мост с получением InstantResponse<string>).
  Рассчитывается, что вы будете возвращать уникальный тип на клиент. 
  Можно оспаривать много такое решение, однако проект еще в разработке и автор стремится сделать библиотеку максимально удобной в использовании.

Основная задача, для которой библиотека была создана - Удобно связать два приложения, BackgroundService и клиент с UI.
