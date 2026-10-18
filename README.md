# ZPI

Projekt zespołowy realizowany w ramach przedmiotu ZPI — aplikacja klient–serwer rozwijana iteracyjnie (sprinty) z wykorzystaniem środowiska agentowego Claude Code.

## Workflow

Każdy sprint przebiega sekwencyjnie, a przejście do kolejnego etapu wymaga akceptacji Lidera:

1. **Lider** dostarcza dokument PDF z wymaganiami do katalogu `wymagania/`.
2. **Analityk** rozpisuje wymagania na user stories z kryteriami akceptacji.
3. **Programista** (full stack) implementuje zaakceptowane user stories.
4. **Recenzent** weryfikuje zaakceptowaną implementację.

Szczegółowe zasady projektu znajdują się w [CLAUDE.md](CLAUDE.md), a opisy ról agentów w katalogu [.claude/agents](.claude/agents).

## Struktura repozytorium

```
wymagania/          dokumenty z wymaganiami na kolejne sprinty
sprinty/sprint-NN/  user stories, raporty implementacji i recenzji
.claude/agents/     definicje agentów: analityk, programista, recenzent
CLAUDE.md           zasady pracy w projekcie
```

## Stos technologiczny

Zostanie ustalony przed pierwszą implementacją.
