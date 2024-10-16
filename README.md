# ⏲️ EDT Vélizy

> [!WARNING]
> This project is done for my **French** school. If you don't have the context, do not bother :)

## 📰 Pourquoi ce project ?

Bien que l'EDT en ligne soit plus que fonctionnel, l'avoir en application, avec enregistrement de la configuration et une actualisation automatique sur le jour actuel, c'est mieux non ?
(Ah et aussi parce que c'est long les cours d'initiation au dév 🤣)

## 🧮 Tech stack

Ce projet se divise en trois "parties" :

- [EDTVelizy.API](https://github.com/ItsTheSky/EDTVelizy/tree/master/EDTVelizy.API) : Ce package regroupe le "cœur" : l'API adaptée en C# des endpoints de l'EDT. Il contient également la base de sérialisation JSON et des requêtes.
- [EDTVelizy.Demo](https://github.com/ItsTheSky/EDTVelizy/tree/master/EDTVelizy.Demo) : Ce package montre un exemple concret et simple à aborder de l'utilisation de l'API (en CLI uniquement)
- [EDTVelizy.Viewer](https://github.com/ItsTheSky/EDTVelizy/tree/master/EDTVelizy.Viewer) : Ce package (et ses sous-packages) contient l'application multi-plateforme pour visionner son EDT.

> [!TIP]
> Le viewer se base sur **Avalonia** (avec **SemiAvalonia** comme thème principal) pour développer une application multi-plateforme en .NET (nan mais sérieusement, Avalonia c'est une tuerie vous devriez essayer :joy:)

## 💿 Crédits

* [@shadowforce78](https://github.com/shadowforce78) (indication des endpoints à utiliser)
* [@Escartem](https://github.com/Escartem/EDTVelizy/blob/master/app/api/getCalendar/route.js) (source originale des endpoints)
