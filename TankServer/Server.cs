using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;
using NLog;

namespace TankServer
{
    public class Server : IDisposable
    {
        protected readonly Random _random;
        protected readonly WebSocketServer _socketServer;

        protected readonly object _syncObject = new object();
        protected DateTime _lastCoreUpdate;

        protected readonly uint _maxBotsCount;
        protected readonly uint _coreUpdateMs;
        protected readonly uint _spectatorUpdateMs;
        protected readonly uint _botUpdateMs;

        public readonly Map Map;
        public Dictionary<IWebSocketConnection, ClientInfo> Clients;

        public readonly Logger _logger;

        public Server(Map map, uint port, uint maxBotsCount, uint coreUpdateMs, uint spectatorUpdateMs, uint botUpdateMs)
        {
            Map = map;
            Clients = new Dictionary<IWebSocketConnection, ClientInfo>();

            _maxBotsCount = maxBotsCount;
            _coreUpdateMs = coreUpdateMs;
            _spectatorUpdateMs = spectatorUpdateMs;
            _botUpdateMs = botUpdateMs;

            _random = new Random();
            _logger = LogManager.GetCurrentClassLogger();

            //FleckLog.Level = LogLevel.Debug;

            _socketServer = new WebSocketServer($"ws://0.0.0.0:{port}");
            _socketServer.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    lock (_syncObject)
                    {
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} [КЛИЕНТ+]: {socket.ConnectionInfo.ClientIpAddress}");
                        _logger.Info($"[КЛИЕНТ+]: {socket.ConnectionInfo.ClientIpAddress}");
                        Clients.Add(socket, new ClientInfo());
                    }
                };
                socket.OnClose = () =>
                {
                    lock (_syncObject)
                    {
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} [КЛИЕНТ-]: {socket.ConnectionInfo.ClientIpAddress}");
                        _logger.Info($"[КЛИЕНТ-]: {socket.ConnectionInfo.ClientIpAddress}");
                        if (Clients.ContainsKey(socket))
                        {
                            Clients[socket].NeedRemove = true;
                        }

                        var firstInQueue = Clients.Values.FirstOrDefault(c => c.IsInQueue);
                        if (firstInQueue != null)
                        {
                            var bot = AddTankBot(firstInQueue.Nickname, firstInQueue.Tag);
                            firstInQueue.IsInQueue = false;
                            firstInQueue.InteractObject = bot;
                        }
                    }
                };
                socket.OnMessage = msg =>
                {
                    lock (_syncObject)
                    {
                        if (!Clients.ContainsKey(socket))
                        {
                            return;
                        }

                        var clientInfo = Clients[socket];

                        var response = msg.FromJson<ServerResponse>();

                        if (response.ClientCommand == ClientCommandType.Logout)
                        {
                            clientInfo.NeedRemove = true;
                        }
                        else
                        {
                            // ответ в этом цикле уже был получен, не обрабатываем до следующего цикла
                            if (clientInfo.Response != null)
                            {
                                return;
                            }

                            clientInfo.Response = response;

                            if (response.ClientCommand == ClientCommandType.Login)
                            {
                                if (clientInfo.IsLogined)
                                {
                                    return;
                                }

                                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} Вход на сервер: {(string.IsNullOrWhiteSpace(response.CommandParameter) ? "наблюдатель" : response.CommandParameter)}");
                                _logger.Info($"Вход на сервер: {(string.IsNullOrWhiteSpace(response.CommandParameter) ? "наблюдатель" : response.CommandParameter)}");

                                clientInfo.IsLogined = true;
                                clientInfo.NeedUpdateMap = true;

                                if (string.IsNullOrWhiteSpace(response.CommandParameter))
                                {
                                    var spectator = AddSpectator();
                                    clientInfo.IsSpecator = true;
                                    clientInfo.InteractObject = spectator;
                                }
                                else
                                {
                                    var nickname = response.CommandParameter;
                                    var tag = string.Empty;
                                    var idx = nickname.IndexOf('\t');
                                    if (idx > 0)
                                    {
                                        tag = nickname.Substring(idx + 1);
                                        nickname = nickname.Substring(0, idx);
                                    }

                                    clientInfo.Nickname = nickname;
                                    clientInfo.Tag = tag;

                                    if (Map.InteractObjects.OfType<TankObject>().Count() >= maxBotsCount)
                                    {
                                        clientInfo.IsInQueue = true;
                                    }
                                    else
                                    {
                                        var bot = AddTankBot(nickname, tag);
                                        clientInfo.InteractObject = bot;
                                    }
                                }
                            }
                        }

                        if (response.ClientCommand != ClientCommandType.None)
                        {
                            Console.WriteLine($"{DateTime.Now.ToShortTimeString()} [КЛИЕНТ]: ответ от {socket.ConnectionInfo.ClientIpAddress} = {response.ClientCommand}");
                            _logger.Info($"[КЛИЕНТ]: ответ от {socket.ConnectionInfo.ClientIpAddress} = {response.ClientCommand}");
                        }
                    }
                };
            });
        }

        public void Dispose()
        {
            foreach (var kv in Clients)
            {
                kv.Key.Close();
            }

            _socketServer.Dispose();
        }

        public BaseInteractObject AddSpectator()
        {
            var spectator = new SpectatorObject(Guid.NewGuid());
            Map.InteractObjects.Add(spectator);

            return spectator;
        }

        public BaseInteractObject AddTankBot(string nickname, string tag)
        {
            Rectangle rectangle;

            while (true)
            {
                var left = _random.Next(Map.MapWidth - Map.CellWidth);
                var top = _random.Next(Map.MapHeight - Map.CellHeight);

                rectangle = new Rectangle(new Point(left, top), Map.CellWidth, Map.CellHeight);

                if (MapManager.GetIntersectedObject(rectangle, Map.InteractObjects) != null)
                {
                    continue;
                }

                var cellTypes = MapManager.WhatOnMap(rectangle, Map);
                if (cellTypes.Any(ct => ct.Value != CellMapType.Void && ct.Value != CellMapType.Grass))
                {
                    continue;
                }

                break;
            }

            var tank = new TankObject(Guid.NewGuid(), rectangle, 2, false, 100, 100, nickname, tag);
            Map.InteractObjects.Add(tank);

            return tank;
        }

        private void AddBullet(TankObject clientTank)
        {
            if(Map.InteractObjects.OfType<BulletObject>().Any(b => b.SourceId == clientTank.Id))
            {
                return;
            }

            var location = new Point(clientTank.Rectangle.LeftCorner);
            switch(clientTank.Direction)
            {
                case DirectionType.Right:
                    location.Left += clientTank.Rectangle.Width;
                    location.Top += clientTank.Rectangle.Height / 2;
                    break;
                case DirectionType.Left:
                    location.Left -= 1;
                    location.Top += clientTank.Rectangle.Height / 2;
                    break;
                case DirectionType.Up:
                    location.Left += clientTank.Rectangle.Width / 2;
                    location.Top -= 1;
                    break;
                case DirectionType.Down:
                    location.Left += clientTank.Rectangle.Width / 2;
                    location.Top += clientTank.Rectangle.Height;
                    break;
            }

            var bullet = new BulletObject(Guid.NewGuid(), new Rectangle(location, 1, 1), clientTank.BulletSpeed, 
                true, clientTank.Direction, clientTank.Id, clientTank.Damage);

            Map.InteractObjects.Add(bullet);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                // запускаем фоновую задачу на изменение игровой карты
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                UpdateEngine(cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                while (true)
                {
                    // сбрасываем данные для клиентов и от клиентов
                    ResetClientsData();

                    // высылаем всем состояние движка
                    await SendUpdates(false, cancellationToken);

                    // в более частом цикле высылаем состояние движка для наблюдателей
                    var botTimer = Convert.ToInt32(_botUpdateMs);
                    while (botTimer > 0 && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(Convert.ToInt32(_spectatorUpdateMs));
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        botTimer -= Convert.ToInt32(_spectatorUpdateMs);
                        await SendUpdates(true, cancellationToken);
                    }

                    // обрабатываем команды от клиентов
                    ApplyClientsData();

                    // удаляем ненужных клиентов
                    RemoveClients();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка во время выполнения: {e}");
            }
        }

        private void RemoveClients()
        {
            lock (_syncObject)
            {
                var socketsToRemove = new List<IWebSocketConnection>();
                foreach (var clientInfo in Clients)
                {
                    if (!clientInfo.Value.NeedRemove)
                    {
                        continue;
                    }

                    if (clientInfo.Value.InteractObject != null)
                    {
                        var objToRemove = Map.InteractObjects.FirstOrDefault(o => o.Id == clientInfo.Value.InteractObject.Id);
                        if (objToRemove != null)
                        {
                            Map.InteractObjects.Remove(objToRemove);
                        }

                        socketsToRemove.Add(clientInfo.Key);
                    }
                }

                foreach (var connection in socketsToRemove)
                {
                    connection.Close();
                    Clients.Remove(connection);
                }
            }
        }

        protected void ResetClientsData()
        {
            lock (_syncObject)
            {
                foreach (var kv in Clients)
                {
                    kv.Value.Request = null;
                    kv.Value.Response = null;
                }
            }
        }

        protected void ApplyClientsData()
        {
            lock (_syncObject)
            {
                foreach (var client in Clients)
                {
                    if (!client.Value.IsLogined || client.Value.IsInQueue)
                    {
                        continue;
                    }

                    var response = client.Value.Response;
                    if (null == response || response.ClientCommand == ClientCommandType.None)
                    {
                        continue;
                    }

                    var clientTank = Map.InteractObjects.OfType<TankObject>().FirstOrDefault(t => t.Id == client.Value.InteractObject.Id);
                    if (null == clientTank && !client.Value.IsSpecator)
                    {
                        continue;
                    }

                    switch (response.ClientCommand)
                    {
                        case ClientCommandType.Logout:
                            client.Value.NeedRemove = true;
                            break;
                        case ClientCommandType.UpdateMap:
                            client.Value.NeedUpdateMap = true;
                            break;
                    }

                    if (client.Value.IsSpecator)
                    {
                        continue;
                    }

                    switch (response.ClientCommand)
                    {
                        case ClientCommandType.Stop:
                            clientTank.IsMoving = false;
                            break;
                        case ClientCommandType.Go:
                            clientTank.IsMoving = true;
                            break;
                        case ClientCommandType.TurnLeft:
                            clientTank.Direction = DirectionType.Left;
                            break;
                        case ClientCommandType.TurnRight:
                            clientTank.Direction = DirectionType.Right;
                            break;
                        case ClientCommandType.TurnUp:
                            clientTank.Direction = DirectionType.Up;
                            break;
                        case ClientCommandType.TurnDown:
                            clientTank.Direction = DirectionType.Down;
                            break;
                        case ClientCommandType.Fire:
                            AddBullet(clientTank);
                            break;
                    }
                }

                ResetClientsData();
            }
        }

        protected DateTime _lastSpectatorsUpd = DateTime.Now;

        protected async Task SendUpdates(bool onlySpectators, CancellationToken cancellationToken)
        {
            List<BaseInteractObject> visibleObjects;
            Map clientMap;

            Dictionary<IWebSocketConnection, ClientInfo> clientsCopy;

            lock (_syncObject)
            {
                visibleObjects = new List<BaseInteractObject>(Map.InteractObjects.Count);

                foreach (var interactObj in Map.InteractObjects.OfType<BaseInteractObject>())
                {
                    if (interactObj is SpectatorObject)
                    {
                        continue;
                    }

                    if (interactObj is TankObject || interactObj is UpgradeInteractObject)
                    {
                        var cells = MapManager.WhatOnMap(interactObj.Rectangle, Map);
                        if (cells.Any(c => c.Value != CellMapType.Grass))
                        {
                            visibleObjects.Add(interactObj);
                        }
                    }
                    else
                    {
                        visibleObjects.Add(interactObj);
                    }
                }

                clientsCopy = new Dictionary<IWebSocketConnection, ClientInfo>(Clients);
            }

            var emptyMap = new Map(null, visibleObjects);

            foreach (var client in clientsCopy)
            {
                if ((onlySpectators && !client.Value.IsSpecator) || !client.Value.IsLogined || client.Value.IsInQueue)
                {
                    continue;
                }

                // запоминаем значение, требовалось высылать ли обновление карты
                var needUpdate = client.Value.NeedUpdateMap;

                clientMap = emptyMap;
                if (needUpdate)
                {
                    lock (_syncObject)
                    {
                        clientMap = new Map(Map, visibleObjects);
                    }
                }

                var request = new ServerRequest
                {
                    Map = clientMap,
                    Tank = client.Value.InteractObject as TankObject
                };

                var json = request.ToJson();

                try
                {
                    await client.Key.Send(json);
                    if (needUpdate)
                    {
                        Console.WriteLine($"{DateTime.Now.ToShortTimeString()} Передача полной карты для {client.Key.ConnectionInfo.ClientIpAddress} / {(client.Value.IsSpecator ? "наблюдатель" : client.Value.Nickname)}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} Ошибка передачи данных клиенту: {e.Message}");
                    client.Value.NeedRemove = true;
                }

                if (needUpdate)
                {
                    // сбрасываем значение, что требовалось ли высылать карту
                    // только в том случае, если мы высылали, т.к. в момент отсылки флаг мог быть сброшен,
                    // но пока шла отсылка - движок мог потребовать обновить карты
                    client.Value.NeedUpdateMap = false;
                }
            }
        }

        public async Task UpdateEngine(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(Convert.ToInt32(_coreUpdateMs));
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_lastCoreUpdate == default(DateTime))
                    {
                        _lastCoreUpdate = DateTime.Now;
                        continue;
                    }

                    var tsDelta = DateTime.Now - _lastCoreUpdate;
                    _lastCoreUpdate = DateTime.Now;
                    var delta = Convert.ToDecimal(tsDelta.TotalSeconds);

                    lock (_syncObject)
                    {
                        AddUpgrades();

                        var objsToRemove = new List<BaseInteractObject>();
                        foreach (var clientInfo in Clients)
                        {
                            if (clientInfo.Value.NeedRemove && clientInfo.Value.InteractObject != null)
                            {
                                var objToRemove = Map.InteractObjects.FirstOrDefault(x => x.Id == clientInfo.Value.InteractObject.Id);
                                if (objToRemove != null)
                                {
                                    objsToRemove.Add(objToRemove);
                                }
                            }
                        }

                        foreach (var movingObject in Map.InteractObjects.OfType<BaseMovingObject>())
                        {
                            if (!movingObject.IsMoving)
                            {
                                continue;
                            }

                            var newPoint = new Point(movingObject.Rectangle.LeftCorner);
                            var newRectangle = new Rectangle(newPoint, movingObject.Rectangle.Width, movingObject.Rectangle.Height);

                            var speed = movingObject.Speed * delta;
                            var shift = speed > 1 ? 1 : speed;

                            while (speed >= 0)
                            {
                                var canMove = newPoint.Left >= 0 &&
                                              newPoint.Left < Map.MapWidth - Constants.CellWidth &&
                                              newPoint.Top >= 0 &&
                                              newPoint.Top < Map.MapHeight - Constants.CellHeight;

                                if (canMove)
                                {
                                    var intersectedObject = MapManager.GetIntersectedObject(newRectangle, Map.InteractObjects.Where(o => o.Id != movingObject.Id));
                                    var cells = MapManager.WhatOnMap(newRectangle, Map);

                                    if (movingObject is BulletObject bulletObject)
                                    {
                                        if (cells.Any(c => c.Value == CellMapType.Wall))
                                        {
                                            objsToRemove.Add(bulletObject);
                                            canMove = false;
                                        }

                                        var destructiveWalls = cells.Where(c => c.Value == CellMapType.DestructiveWall).ToList();
                                        if (destructiveWalls.Count > 0)
                                        {
                                            foreach (var destructiveWall in destructiveWalls)
                                            {
                                                Map.Cells[destructiveWall.Key.TopInt, destructiveWall.Key.LeftInt] = CellMapType.Void;
                                            }

                                            objsToRemove.Add(bulletObject);

                                            // т.к. изменилась карта, то надо всем клиентам выслать новую карту
                                            foreach (var clientInfo in Clients)
                                            {
                                                clientInfo.Value.NeedUpdateMap = true;
                                            }

                                            canMove = false;
                                        }

                                        if (intersectedObject is TankObject tankIntersectedObject)
                                        {
                                            objsToRemove.Add(bulletObject);
                                            canMove = false;

                                            var hpToRemove = tankIntersectedObject.Hp > bulletObject.DamageHp
                                                ? bulletObject.DamageHp
                                                : tankIntersectedObject.Hp;
                                            bool isFrag = false;
                                            tankIntersectedObject.Hp -= hpToRemove;
                                            if (tankIntersectedObject.Hp <= 0)
                                            {
                                                isFrag = true;
                                                objsToRemove.Add(tankIntersectedObject);
                                            }

                                            var sourceTank = Map.InteractObjects.OfType<TankObject>()
                                                .FirstOrDefault(t => t.Id == bulletObject.SourceId);
                                            if (sourceTank != null)
                                            {
                                                sourceTank.Score += hpToRemove;
                                                if (isFrag)
                                                {
                                                    sourceTank.Score += 50;
                                                }
                                            }
                                        }
                                        else if (intersectedObject is BulletObject bulletIntersectedObject)
                                        {
                                            objsToRemove.Add(bulletObject);
                                            objsToRemove.Add(bulletIntersectedObject);
                                        }
                                    }
                                    else if (movingObject is TankObject tankObject)
                                    {
                                        if (cells.Any(c => c.Value == CellMapType.DestructiveWall || c.Value == CellMapType.Wall))
                                        {
                                            canMove = false;
                                        }
                                        else if ((decimal) cells.Count(c => c.Value == CellMapType.Water) / cells.Count >= 0.5m)
                                        {
                                            objsToRemove.Add(tankObject);
                                            canMove = false;
                                        }

                                        if (intersectedObject is UpgradeInteractObject upgradeObject)
                                        {
                                            var tank = movingObject as TankObject;
                                            switch (upgradeObject.Type)
                                            {
                                                case UpgradeType.BulletSpeed:
                                                {
                                                    var upgrade = upgradeObject as BulletSpeedUpgradeObject;
                                                    tank.BulletSpeed += upgrade.IncreaseBulletSpeed;
                                                    break;
                                                }
                                                case UpgradeType.Damage:
                                                {
                                                    var upgrade = upgradeObject as DamageUpgradeObject;
                                                    tank.Damage += upgrade.IncreaseDamage;
                                                    break;
                                                }
                                                case UpgradeType.Health:
                                                {
                                                    var upgrade = upgradeObject as HealthUpgradeObject;
                                                    var newHP = tank.Hp + upgrade.RestHP;
                                                    tank.Hp = newHP > tank.MaximumHp ? tank.MaximumHp : newHP;
                                                    break;
                                                }
                                                case UpgradeType.MaxHp:
                                                {
                                                    var upgrade = upgradeObject as MaxHpUpgradeObject;
                                                    tank.MaximumHp += upgrade.IncreaseHP;
                                                    tank.Hp += upgrade.IncreaseHP;
                                                    break;
                                                }
                                            }

                                            objsToRemove.Add(intersectedObject);
                                        }

                                        if (canMove)
                                        {
                                            canMove = intersectedObject == null;
                                        }
                                    }
                                }

                                if (canMove)
                                {
                                    movingObject.Rectangle = new Rectangle(newRectangle);

                                    switch (movingObject.Direction)
                                    {
                                        case DirectionType.Left:
                                            newPoint.Left -= shift;
                                            break;
                                        case DirectionType.Right:
                                            newPoint.Left += shift;
                                            break;
                                        case DirectionType.Up:
                                            newPoint.Top -= shift;
                                            break;
                                        case DirectionType.Down:
                                            newPoint.Top += shift;
                                            break;
                                    }

                                    speed = speed - shift;
                                }
                                else
                                {
                                    if (movingObject is BulletObject)
                                    {
                                        objsToRemove.Add(movingObject);
                                    }

                                    break;
                                }
                            }
                        }

                        foreach (var objToRemove in objsToRemove)
                        {
                            var client = Clients.FirstOrDefault(c => c.Value.InteractObject != null && c.Value.InteractObject.Id == objToRemove.Id);
                            if (client.Key != null)
                            {
                                client.Value.NeedRemove = true;
                            }
                            else
                            {
                                Map.InteractObjects.Remove(objToRemove);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Ошибка работы игрового движка: {e}");
                }
            }
        }

        protected void AddUpgrades()
        {
            var rnd = _random.NextDouble();
            if (rnd <= 0.995)
            {
                return;
            }

            if (Map.InteractObjects.OfType<UpgradeInteractObject>().Count() >= 3)
            {
                return;
            }

            Rectangle rectangle;

            while (true)
            {
                var left = _random.Next(Map.MapWidth - Map.CellWidth);
                var top = _random.Next(Map.MapHeight - Map.CellHeight);

                rectangle = new Rectangle(new Point(left, top), Map.CellWidth, Map.CellHeight);

                if (MapManager.GetIntersectedObject(rectangle, Map.InteractObjects) != null)
                {
                    continue;
                }

                var cellTypes = MapManager.WhatOnMap(rectangle, Map);
                if (cellTypes.Any(ct => ct.Value != CellMapType.Void && ct.Value != CellMapType.Grass))
                {
                    continue;
                }

                break;
            }

            var upgradeObj = GetUpgradeToCreate(rectangle);
            Map.InteractObjects.Add(upgradeObj);
        }

        protected UpgradeInteractObject GetUpgradeToCreate(Rectangle rectangle)
        {
            var rnd = _random.Next(1, Enum.GetValues(typeof(UpgradeType)).Length);
            var objId = Guid.NewGuid();
            UpgradeInteractObject result;

            switch (rnd)
            {
                case 1:
                    result = new BulletSpeedUpgradeObject(objId, rectangle);
                    break;
                case 2:
                    result = new DamageUpgradeObject(objId, rectangle);
                    break;
                case 3:
                    result = new HealthUpgradeObject(objId, rectangle);
                    break;
                case 4:
                    result = new MaxHpUpgradeObject(objId, rectangle);
                    break;
                case 5:
                    result = new SpeedUpgradeObject(objId, rectangle);
                    break;
                default:
                    throw new NotSupportedException("Incorrect object");
            }

            return result;
        }
    }
}
