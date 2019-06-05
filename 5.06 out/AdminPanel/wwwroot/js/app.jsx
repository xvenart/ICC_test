var mapTypeList = [];
var serverList = [
    { 'id': 1, 'name': 1, 'port': '2000', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' },
    { 'id': 2, 'name': 1, 'port': '2010', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' },
    { 'id': 3, 'name': 1, 'port': '2020', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' },
    { 'id': 4, 'name': 1, 'port': '2030', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' },
    { 'id': 5, 'name': 1, 'port': '2040', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' },
    { 'id': 6, 'name': 1, 'port': '2050', 'ip': 2000, 'type': 'Tank', 'people': '0 / 0' }
];
var serverTypeList = [];

class UserForm extends React.Component {
    postHelper(method, data) {
        var form = new FormData();
        if (data)
            form.append('request', data);

        var requestType = { method: "POST", body: form };
        var url = `admin/${method}`;

        return fetch(url, requestType).then((x) => { if(x.status === 200) return x.json(); });
    }

    getHelper(method) {
        var requestType = { method: "GET" };
        var url = `admin/${method}`;
        return fetch(url, requestType).then((x) => { return x.json(); });
    }

    constructor(props) {
        super(props);

        this.state = {
            viewModal: false,
            serverList: [],
            viewTankSettings: false,
            error: "",
            isAdmin: false
        };

        this.openModal = this.openModal.bind(this);
        this.doTestStart = this.doTestStart.bind(this);
        this.getList = this.getList.bind(this);
        this.getText = this.getText.bind(this);
        this.updateServerList = this.updateServerList.bind(this);
        this.editTankSettings = this.editTankSettings.bind(this);
        this.viewTankSettings = this.viewTankSettings.bind(this);
        this.stopServer = this.stopServer.bind(this);
    }

    getList(method) {
        var list = [];
        this.getHelper(method).then(x => {
            if (x.length > 1) x.forEach(z => list.push(z));
            else
                list.push(x[0]);
        });
        return list;
    }

    componentDidMount() {
        mapTypeList = this.getList('GetMapTypes');
        serverTypeList = this.getList('GetServerTypes');
        setInterval(() => this.updateServerList(), 1000);
        if (location.hostname === "localhost" || location.hostname === "127.0.0.1") {
            this.setState({ isAdmin: true });
        }
    }

    updateServerList() {
        let list = this.getList('GetServerList');
        setTimeout(() => this.setState({ serverList: list }), 200);
    }

    getText(div) {
        var result = [];
        div.childNodes.forEach(z => {
            if (z.localName === 'div') {
                if (z.firstChild.localName === 'div') {
                    result.push('' + z.id + ': ' + this.getText(z));
                } else result.push('' + z.id + ': "' + z.lastChild.value + '"');
            }
        });
        return ('{ ' + result.join(', ') + ' }');
    }

    doTestStart() {
        this.setState({ error: "" });
        var result = this.getText(document.getElementById('modal'));

        this.postHelper("CreateServer", result).then(x => {
            if (x && x.error) {
                this.setState({ error: x.error });
            }
            else {
                this.openModal();
            }
        });
    }

    openModal() {
        var view = !this.state.viewModal;
        this.setState({ viewModal: view, viewTankSettings: view, error: "" });
    }

    editTankSettings() {
        var id = document.getElementById('ServerId').value;
        var result = this.getText(document.getElementById('TankSettings'));
        this.viewTankSettings();

        this.postHelper(`ChangeServerSettings?id=${id}`, result);
    }

    viewTankSettings() {
        this.setState({ viewTankSettings: !this.state.viewTankSettings });
    }

    stopServer() {
        var id = document.getElementById('ServerId').value;
        this.postHelper(`StopServer?id=${id}`);
    }

    render() {
        return (
            <div>
                <table className="table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Ip</th>
                            <th>Port</th>
                            <th>Type</th>
                            <th>People</th>
                        </tr>
                    </thead>
                    <tbody>{this.state.serverList.map(p => {
                        if (p) 
                        return (
                            <tr key={p.Id}>
                                {Object.keys(p).filter(k => k !== 'Id').map(k => { return (<td><div>{p[k]}</div></td>); })}
                            </tr>
                        );
                    })}</tbody>
                </table>
                <select className={this.state.isAdmin ? 'custom-select' : 'invisible'} type="text" defaultValue="0" id="ServerId">{this.state.serverList.map(e => {
                    if (e)
                    return <option value={e.Id}>{e.Name}</option>;
                })}</select>
                <button className={this.state.viewModal || !this.state.isAdmin ? 'btn btn-primary invisible' : 'btn btn-primary visible'} disabled={!this.state.serverList[0]} id="ChangeServer" onClick={this.viewTankSettings}>{this.state.viewTankSettings ? 'Cancel' : 'Edit'}</button>
                <button className={this.state.viewTankSettings || !this.state.isAdmin ? 'btn btn-primary invisible' : 'btn btn-primary visible'} disabled={!this.state.serverList[0]} id="StopServer" onClick={this.stopServer}>Stop Server</button>
                <br/>
                <button className={this.state.viewModal === this.state.viewTankSettings && this.state.isAdmin ? 'btn btn-primary visible' : 'btn btn-primary invisible'} id="AddServer" onClick={this.openModal}>{this.state.viewModal ? 'Close' : 'Add'}</button>
                <div id="errorMessage" className={this.state.error ? 'visible' : 'invisible'}>{this.state.error}</div>
                <div id="modal" className={this.state.isAdmin ? 'visible' : 'invisible'}>
                    <div id="SessionName" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Имя сервера</label>
                        <input type="text" id="Value" />
                    </div>
                    <div id="MapType" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Тип шаблона карты</label>
                        <select className="custom-select" type="text" defaultValue="0" id="Value">{mapTypeList.map(e => {
                            return <option value={e.id}>{e.name}</option>;
                        })}</select>
                    </div>
                    <div id="Width" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Ширина карты</label>
                        <input type="number" defaultValue="30" id="Value" />
                    </div>
                    <div id="Height" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Высота карты</label>
                        <input type="number" defaultValue="30" id="Value" />
                    </div>
                    <div id="MaxClientCount" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Максимальное количество игроков</label>
                        <input type="number" defaultValue="10" id="Value" />
                    </div>
                    <div id="CountOfLifes" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Количество жизней танков</label>
                        <input type="number" defaultValue="3" id="Value" />
                    </div>
                    <div id="TimeOfInvulnerabilityAfterRespawn" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Время неуязвимости после перерождения (ms)</label>
                        <input type="number" defaultValue="5000" id="Value" />
                    </div>
                    <div id="MaxCountOfUpgrade" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Максимальное количество бонусов на карте</label>
                        <input type="number" defaultValue="3" id="Value" />
                    </div>
                    <div id="ServerType" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Тип сервера</label>
                        <select className="custom-select" type="text" defaultValue="0" id="Value">{serverTypeList.map(e => {
                            return <option value={e.id}>{e.name}</option>;
                        })}</select>
                    </div>
                    <div id="SecondsToDespawn" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Время до деспауна бонусов</label>
                        <input type="number" defaultValue="30" id="Value" />
                    </div>
                    <div id="IsMultipleConnectionAllow" className={this.state.viewModal ? 'visible' : 'invisible'}>
                        <label>Несколько клиентов с 1 ip</label>
                        <select className="custom-select" defaultValue="0" id="Value">
                        <option value="false">Запретить</option>
                        <option value="true">Разрешить</option></select>
                    </div>
                    <div id="TankSettings" className={this.state.viewTankSettings ? 'visible' : 'invisible'}>
                        <div id="StartSesison">
                            <label>Начало игровой сессии</label>
                            <input type="datetime-local" defaultValue="2019-05-31T16:00" id="Value" />
                        </div>
                        <div id="FinishSesison">
                            <label>Конец игровой сессии</label>
                            <input type="datetime-local" defaultValue="2019-05-31T17:30" id="Value" />
                        </div>
                        <div id="GameSpeed">
                            <label>Скорость игры</label>
                            <input type="number" defaultValue="1" id="Value" />
                        </div>
                        <div id="TimeOfActionUpgrades">
                            <label>Время действия бонусов (ms)</label>
                            <input type="number" defaultValue="5000" id="Value" />
                        </div>
                        <div id="ChanceSpawnUpgrades">
                            <label>Шанс появления бонусов</label>
                            <input type="number" defaultValue="0.05" id="Value" />
                        </div>
                        <div id="TankSpeed">
                            <label>Множитель скорости танка</label>
                            <input type="number" defaultValue="2" id="Value" />
                        </div>
                        <div id="BulletSpeed">
                            <label>Множитель скорости пули</label>
                            <input type="number" defaultValue="4" id="Value" />
                        </div>
                        <div id="TankDamage">
                            <label>Урон танка</label>
                            <input type="number" defaultValue="40" id="Value" />
                        </div>
                        <div id="TankMaxHP">
                            <label>Максимальный HP танков</label>
                            <input type="number" defaultValue="100" id="Value" />
                        </div>
                        <div id="IncreaseBulletSpeed">
                            <label>Показатели бонуса увеличения скорости пуль</label>
                            <input type="number" defaultValue="2" id="Value" />
                        </div>
                        <div id="IncreaseDamage">
                            <label>Показатели бонуса увеличения урона танков</label>
                            <input type="number" defaultValue="20" id="Value" />
                        </div>
                        <div id="RestHP">
                            <label>Показатели бонуса лечения</label>
                            <input type="number" defaultValue="25" id="Value" />
                        </div>
                        <div id="TimeOfInvulnerability">
                            <label>Показатели бонуса неуязвимости (ms)</label>
                            <input type="number" defaultValue="5000" id="Value" />
                        </div>
                        <div id="IncreaseHP">
                            <label>Показатели бонуса увеличения максимального количества HP</label>
                            <input type="number" defaultValue="25" id="Value" />
                        </div>
                        <div id="IncreaseSpeed">
                            <label>Показатели бонуса увеличения скорости танка</label>
                            <input type="number" defaultValue="1" id="Value" />
                        </div>
                    </div>
                </div>
                <button id="AddServer" className={this.state.isAdmin && this.state.viewTankSettings !== this.state.viewModal ? 'btn btn-primary visible' : 'btn btn-primary invisible'} onClick={this.editTankSettings}>Change</button>
                <button id="AddServer" className={this.state.viewModal && this.state.isAdmin ? 'btn btn-primary visible' : 'btn btn-primary invisible'} onClick={this.doTestStart}>Create</button>
                <div id="links" className={this.state.isAdmin ? 'invisible' : 'visible'}>
                    <a href="clients/cs.zip"><div id="client"><img id="logo" src="clients/cs.svg" /><p>C# - клиент</p></div></a>
                    <a href="clients/java.zip"><div id="client"><img id="logo" src="clients/java.svg" /><p>Java - клиент</p></div></a>
                    <a href="clients/js.zip"><div id="client"><img id="logo" src="clients/js.svg" /><p>JS - клиент</p></div></a>
                    <a href="clients/py.zip"><div id="client"><img id="logo" src="clients/py.svg" /><p>Python - клиент</p></div></a>
                </div>
                <a href="clients/observer.zip" className={this.state.isAdmin ? 'invisible' : 'visible'}><div id="observer"><img id="logo" src="clients/binoculars.svg" /><p>Gui - наблюдатель</p></div></a>
            </div >
        );
    }
}
ReactDOM.render(
    <UserForm />,
    document.getElementById("app")
)