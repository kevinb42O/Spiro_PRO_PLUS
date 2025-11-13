# Multi-Agent Spirograph System Documentation

## Overview

The Multi-Agent Spirograph System transforms the single-threaded spirograph drawing into a collaborative multi-agent art machine where up to 16 agents can draw simultaneously on the same path.

## Architecture

### Master-Slave Architecture

The system uses a master-slave architecture where:
- **SharedPathState**: Single source of truth - holds master settings that all agents read
- **PathAgent**: Individual agents that follow the master state
- **MultiAgentManager**: Orchestrates agent spawning, lifecycle, and coordination

### Components

#### 1. SharedPathState.cs
**Purpose**: Central state manager for all agents

**Key Properties**:
- `masterSpeed`: Travel speed (0-750)
- `masterRotationSpeed`: Rotation speed (0-1)
- `masterPenDistance`: Pen arm distance (0-5)
- `masterCycles`: Number of cycles to complete
- `masterLineWidth`: Trail line width
- `masterLineBrightness`: Trail opacity
- `masterLineColor`: Base color for agents
- `pathPoints`: The path all agents follow

**Methods**:
- `GetAgentColor(colorMode, index, totalAgents)`: Calculate agent color based on mode
- `UpdateMasterColorFromHSV(h, s, v)`: Update color from HSV sliders

#### 2. PathAgent.cs
**Purpose**: Individual agent that draws on the path

**Key Properties**:
- `agentIndex`: Agent identifier
- `agentName`: Human-readable name
- `sharedState`: Reference to SharedPathState
- `startPositionPercent`: Starting position (0-1)
- `speedMultiplier`: Speed variation for competitive mode
- `isPaused`: Individual pause state
- `status`: AgentStatus (Idle/Active/Paused/Completed)
- `agentColor`: This agent's color
- `segmentStart/segmentEnd`: Segment boundaries (for staggered mode)

**Methods**:
- `StartDrawing()`: Begin drawing
- `Pause()`: Pause this agent
- `Resume()`: Resume this agent
- `ResetAgent()`: Reset to starting position
- `SetColor(color)`: Change agent color
- `UpdateLineWidth(width)`: Update trail width
- `HighlightTrail(highlight)`: Highlight/unhighlight trail

**Lifecycle**:
1. **Idle**: Created but not started
2. **Active**: Currently drawing
3. **Paused**: Temporarily stopped
4. **Completed**: Finished all cycles

#### 3. MultiAgentManager.cs
**Purpose**: Manages all agents in the system

**Key Properties**:
- `sharedState`: Reference to SharedPathState
- `agentCount`: Number of agents (1-16)
- `agentPrefab`: Optional agent prefab
- `autoCreateAgents`: Auto-create if no prefab
- `colorMode`: AgentColorMode (Master/Rainbow/Individual/Custom)
- `spawnMode`: AgentSpawnMode (Simultaneous/Sequential/Staggered/Competitive)
- `agents`: List of all active agents
- `selectedAgent`: Currently selected agent
- Global stats: activeAgentCount, pausedAgentCount, completedAgentCount

**Color Modes**:
- **Master**: All agents use master color
- **Rainbow**: Agents distributed across hue spectrum
- **Individual**: Each agent gets predefined palette color
- **Custom**: Manual color assignment per agent

**Spawn Modes**:
- **Simultaneous**: All agents start together at position 0
- **Sequential**: Agents start one after another with delay
- **Staggered**: Agents distributed evenly along path
- **Competitive**: Agents start together with random speed variations

**Methods**:
- `EnableMultiAgentMode()`: Start multi-agent system
- `DisableMultiAgentMode()`: Stop and clean up
- `SpawnAgents()`: Create all agents based on config
- `SelectAgent(index)`: Select agent for camera follow
- `PauseAgent(index)`: Pause specific agent
- `ResumeAgent(index)`: Resume specific agent
- `PauseAllAgents()`: Pause all
- `ResumeAllAgents()`: Resume all
- `ResetAllAgents()`: Reset all to start
- `SetAgentCount(count)`: Change agent count (respawns)
- `SetColorMode(mode)`: Change color mode (recolors)
- `SetSpawnMode(mode)`: Change spawn mode (respawns)

#### 4. AgentPanelUI.cs
**Purpose**: Bottom-right UI panel for agent management

**Features**:
- **Global Stats Display**: Shows active/paused/completed counts, average progress, total distance
- **Agent Roster**: Scrollable list of agent cards
- **Per-Agent Controls**: Focus, Pause/Resume, Select
- **Real-time Updates**: Stats update at 10 Hz for performance

**Agent Card Features**:
- Agent name in agent color
- Status indicator (●/▶/⏸/✓)
- Progress bar
- Live stats (progress %, speed multiplier)
- Focus button (🔍): Snap camera to agent
- Pause/Resume button: Individual control
- Radio button: Select for camera follow

**Color Coding**:
- Idle: Dark gray
- Active: Green tint
- Paused: Orange tint  
- Completed: Blue tint

**Methods**:
- `CreatePanel(canvas)`: Generate panel UI
- `PopulateAgentList()`: Create cards for all agents
- `ShowPanel()`: Display panel
- `HidePanel()`: Hide panel

#### 5. SpirographUIManager.cs (Extended)
**Added Features**:

**Multi-Agent Controls (Environment Section)**:
- Multi-Agent Mode toggle
- Agent Count slider (1-16)
- Agent Color Mode dropdown
- Agent Spawn Mode dropdown

**Control Flow**:
- When toggle OFF: Sliders control activeRoller (single mode)
- When toggle ON: Sliders control SharedPathState (multi-agent mode)
- Visual feedback: Speed text shows "(ALL)" in multi-agent mode

**New Methods**:
- `CreateModernToggle()`: Create glassmorphic checkbox
- `CreateCompactDropdown()`: Create inline dropdown
- `ConnectMultiAgentSystem()`: Wire up multi-agent UI
- `OnMultiAgentModeToggled()`: Handle mode switch
- `ConnectSlidersToSharedState()`: Redirect sliders to shared state

## Usage Instructions

### Setup

1. **Generate UI** (if not done):
   - Create empty GameObject → Add SpirographUIManager
   - Check "Generate UI" in inspector
   - UI will auto-create, including agent panel

2. **Create a Pattern**:
   - Use Pattern Generator dropdown
   - Click "✦ GENERATE" button
   - Pattern and rotor will be created

3. **Enable Multi-Agent Mode**:
   - Expand Environment section in left panel
   - Toggle "Enable Multi-Agent Mode"
   - Agent panel appears bottom-right

### Configuration

1. **Set Agent Count**: Use slider (1-16 agents)
2. **Choose Color Mode**: 
   - Master: All same color
   - Rainbow: Spectrum distribution
   - Individual: Palette colors
   - Custom: Manual assignment

3. **Choose Spawn Mode**:
   - Simultaneous: Race from same start
   - Sequential: Start with delays
   - Staggered: Distributed along path
   - Competitive: Random speed variations

4. **Adjust Master Settings**:
   - All sliders now control ALL agents
   - Speed, Rotation, Pen Distance, Cycles
   - Line Width, Brightness, Color

### Agent Controls

**From Agent Panel**:
- **🔍 Focus**: Snap camera to agent
- **⏸ Pause / ▶ Resume**: Control individual agent
- **● Radio Button**: Select for camera follow
- **Card Click**: View agent details

**Global Controls**:
- All controls in left panel affect all agents simultaneously
- Color picker changes master color (in Master mode)

### Camera Control

- **Free Fly**: WASD movement, right-click to look
- **Smooth Follow**: Follows selected agent
- **Auto Orbit**: Orbits around scene
- **Focus Button**: Quick snap to agent

## Performance Optimization

### For 16 Agents

1. **Stats Update Rate**: 10 Hz (not 60 Hz)
2. **Dirty Flags**: Only update changed values
3. **Object Pooling**: Reuse trail renderers
4. **Efficient Path Caching**: Pre-calculate path once
5. **Batch Updates**: Update all agents in single loop

### Recommended Settings

- **1-4 agents**: Full quality, no compromises
- **5-8 agents**: Normal performance
- **9-12 agents**: Monitor frame rate
- **13-16 agents**: May need performance tweaks

## Creative Use Cases

### Visual Art
- Rainbow mode with 8 agents: Prismatic patterns
- Staggered mode: Progressive reveal of pattern
- Competitive mode: Organic, varied drawing

### Performance Analysis
- Compare agent speeds and completion times
- Visualize segment distribution
- Track progress metrics

### Debugging
- Focus on specific agent trail
- Pause problematic agents
- Isolate issues to specific agents

### Cinematic
- Sequential spawn for time-lapse effect
- Camera follow racing agents
- Browse completed agent trails

## Technical Details

### Segment Distribution (Staggered Mode)

```
Agent 0: 0.00 - 0.25 (0-25%)
Agent 1: 0.25 - 0.50 (25-50%)
Agent 2: 0.50 - 0.75 (50-75%)
Agent 3: 0.75 - 1.00 (75-100%)
```

Each agent is responsible for drawing a specific portion of the path.

### Speed Variation (Competitive Mode)

```
Base Speed: 50
Variation: ±20%
Agent Speeds: 40, 45, 52, 58, 55, 47, 60, 43
```

Creates natural racing dynamics.

### Color Distribution (Rainbow Mode)

```
Agent 0: Hue 0.00 (Red)
Agent 1: Hue 0.25 (Green)
Agent 2: Hue 0.50 (Cyan)
Agent 3: Hue 0.75 (Purple)
```

Evenly distributed across color spectrum.

## Troubleshooting

### Agents Not Appearing
- Ensure pattern exists before enabling multi-agent mode
- Check SharedPathState has pathPoints assigned
- Verify MultiAgentManager.isMultiAgentMode = true

### Agents Not Moving
- Check masterSpeed > 0
- Ensure agents are in Active state (not Paused or Idle)
- Verify agent.StartDrawing() was called

### UI Not Updating
- Confirm AgentPanelUI is active
- Check MultiAgentManager reference is set
- Verify Update() is being called

### Performance Issues
- Reduce agent count
- Lower line quality settings
- Disable unused visual effects
- Check for excessive trail vertex counts

## API Reference

### Key Events

```csharp
// MultiAgentManager
OnAgentSelected(PathAgent agent)  // Agent selected for follow
OnAgentCompleted(PathAgent agent) // Agent finished all cycles

// AgentPanelUI  
// (Uses MultiAgentManager events)
```

### Key Methods

```csharp
// Enable/Disable System
multiAgentManager.EnableMultiAgentMode();
multiAgentManager.DisableMultiAgentMode();

// Agent Control
multiAgentManager.SelectAgent(index);
multiAgentManager.PauseAgent(index);
multiAgentManager.ResumeAgent(index);
multiAgentManager.ResetAllAgents();

// Configuration
multiAgentManager.SetAgentCount(count);
multiAgentManager.SetColorMode(mode);
multiAgentManager.SetSpawnMode(mode);

// Agent Panel
agentPanelUI.ShowPanel();
agentPanelUI.HidePanel();
agentPanelUI.PopulateAgentList();
```

## Future Enhancements

Potential features for future versions:

1. **Agent Personalities**: Individual behavior patterns
2. **Collaborative Effects**: Agents react to nearby agents
3. **Path Optimization**: Agents avoid crossing trails
4. **Agent Communication**: Visual links between agents
5. **Save/Load Presets**: Store agent configurations
6. **Agent Replay**: Rewind/playback individual agents
7. **Custom Agent Shapes**: Different rotor geometries
8. **Trail Effects**: Per-agent effect modes
9. **Particle Systems**: Winner effects in competitive mode
10. **Sound Synthesis**: Audio feedback from agent progress

## Credits

Multi-Agent Spirograph System
Version 1.0
Implements master-slave architecture for collaborative spirograph drawing
