﻿using ServerTCP;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using Zenject;

namespace Room
{
    public class RoomGameInstaller : MonoInstaller
    {
        public async override void InstallBindings()
        {
            Container.Bind<Settings>().AsCached();

            

            Container.Bind<LampPresenter>().AsCached();
            Container.Bind<Lamp>().AsCached();

            Container.Bind<Explosion>().AsCached();
            Container.Bind<ExplosionPresenter>().AsCached();

            var settings = Container.Resolve<Settings>();

            Container.Bind<Server>().AsCached().WithArguments(settings.Port, Debug.unityLogger);

            var lamp = Container.Resolve<Lamp>();
            var explosion = Container.Resolve<Explosion>();
            var server = Container.Resolve<Server>();

            server.ReplySubject
               .Subscribe(x => Debug.Log($"{nameof(server.ReplySubject)} received {BitConverter.ToString(x)}"));
               

            server.ReplySubject
                .Where(TypeMessage.IsLightSwitchMsg)
                .Select(TypeMessage.ConvertLightSwitchMsg)
                .Do(x => Debug.Log($"{nameof(lamp)} received {x}"))
                .Subscribe(state => lamp.IsTurnOn.SetValueAndForceNotify(state));

            server.ReplySubject
                .Where(TypeMessage.IsBlowUpMessage)
                .Do(x => Debug.Log($"{nameof(explosion)} received {BitConverter.ToString(x)}"))
                .Subscribe(_ => explosion.BlowUp.Execute());

            var cts = new CancellationTokenSource();
            await server.ListenForIncommingRequests(cts.Token);
        }
    }
}