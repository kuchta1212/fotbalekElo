/**
 * Domain Types for Elo-fotbalek
 * These types mirror the backend models and API contracts
 */

export type Season = 'Winter' | 'Summer' | 'Overall';

export interface Player {
  id: string;
  name: string;
  elo: number;
  winterElo: number;
  summerElo: number;
  overallElo: number;
  matchesPlayed: number;
  wins: number;
  losses: number;
  isRegular?: boolean;
}

export interface SeasonalElos {
  winter: number;
  summer: number;
  overall: number;
}

export interface Match {
  id: string;
  date: string; // ISO date string
  season: Season;
  isSmallMatch: boolean;
  teamA: string[]; // Player IDs
  teamB: string[]; // Player IDs
  teamAScore: number;
  teamBScore: number;
  winner: 'A' | 'B';
  jirkaLunak?: string; // Player ID (optional)
}

export interface Team {
  players: Player[];
  averageElo: number;
  totalElo: number;
}

export interface GeneratorResult {
  teamA: Team;
  teamB: Team;
  eloDifference: number;
}

export interface DoodleValue {
  playerId: string;
  playerName: string;
  status: 'Yes' | 'No' | 'Maybe';
}

export interface Doodle {
  date: string; // ISO date string (YYYY-MM-DD)
  values: DoodleValue[];
}

export interface DoodleStats {
  yesCount: number;
  maybeCount: number;
  noCount: number;
}

export interface LeaderboardEntry {
  player: Player;
  rank: number;
  trend?: 'up' | 'down' | 'stable';
}

export interface PlayerStats {
  player: Player;
  highestElo: number;
  lowestElo: number;
  currentElo: SeasonalElos;
  attendanceCount: number;
  eloHistory: TrendData[];
}

export interface TrendData {
  date: string; // ISO date string
  elo: number;
  matchId?: string;
}

export interface MatchCounter {
  totalMatches: number;
  bigMatches: number;
  smallMatches: number;
}

export interface ChartDataPoint {
  date: string;
  elo: number;
  label?: string;
}
