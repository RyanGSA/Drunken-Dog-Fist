# Divisão de Tarefas — Drunken Dog Fist

**Disciplina:** Introdução ao Desenvolvimento de Jogos  
**Engine:** Unity | **Plataforma:** PC

**GDD:** https://docs.google.com/document/d/10gOtCD9slmQS_b-CSi_CoCBgaaR1w_iDWzPIbAbOLlI/edit?usp=sharing

---

## Visão Geral (IDEIA)

| 🎮 Ryan — Core + Jogador | ⚔️ Alex — Inimigos + IA Combate | 🎨 Marcelo — Fases + UI + Arte |
|:---:|:---:|:---:|
| PlayerController | EnemyBase | LevelManager |
| PlayerCombat | Capanga / Segurança / Mafioso | RoomManager |
| PlayerMovement | Boss AI / MiniBoss AI | CheckpointSystem |
| ComboSystem | WaveSpawner | HealthBarUI |
| InputManager | DamageSystem | CutsceneManager |
| Dash / Esquiva | HitboxManager | Cenários (3 fases) |
| GameManager | Cachorros pós-Boss | Menu Principal |
| | | Garrafas de Vida |

---

## 🎮 Ryan (Programação Core + Jogador)

Responsável por tudo que o jogador faz: mover, atacar, defender, combos.

---

## ⚔️ Alex (Inimigos + IA de Combate)

Responsável por todos os inimigos, suas IAs, e os sistemas de dano/hitbox.

---

## 🎨 Marcelo (Fases + UI + Direção de Arte)

Responsável pelo level design, interface visual, cutscenes, cenários.

---
