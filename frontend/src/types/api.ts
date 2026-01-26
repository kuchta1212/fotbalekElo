/**
 * API Request/Response DTOs
 */

import type { Season, Player, Match, GeneratorResult, Doodle, LeaderboardEntry, PlayerStats } from './domain';

// API Response wrapper
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  error?: string;
}

// Leaderboard API
export interface LeaderboardResponse {
  regularPlayers: LeaderboardEntry[];
  nonRegularPlayers: LeaderboardEntry[];
  season: Season;
}

// Players API
export interface PlayersListResponse {
  players: Player[];
}

export interface PlayerDetailResponse {
  player: Player;
  stats: PlayerStats;
}

// Matches API
export interface MatchesListResponse {
  matches: Match[];
  total: number;
}

export interface MatchDetailResponse {
  match: Match;
  teamAPlayers: Player[];
  teamBPlayers: Player[];
  jirkaLunakPlayer?: Player;
}

export interface MatchFilters {
  season?: Season;
  dateFrom?: string;
  dateTo?: string;
  isSmallMatch?: boolean;
  skip?: number;
  take?: number;
}

// Doodle API
export interface DoodleListResponse {
  doodles: Doodle[];
  upcomingDates: string[];
}

export interface DoodleDetailResponse {
  doodle: Doodle;
  stats: {
    yesCount: number;
    maybeCount: number;
    noCount: number;
  };
}

export interface UpdateAvailabilityRequest {
  playerId: string;
  status: 'Yes' | 'No' | 'Maybe';
}

// Team Generator API
export interface GenerateTeamsRequest {
  date: string;
  season: Season;
  playerIds?: string[]; // Optional: override doodle selection
}

export interface GenerateTeamsResponse {
  results: GeneratorResult[];
  season: Season;
  date: string;
  totalOptions: number;
}

// Admin API
export interface AddPlayerRequest {
  name: string;
  initialElo?: number;
}

export interface AddMatchRequest {
  date: string; // ISO datetime string
  season: Season;
  isSmallMatch: boolean;
  teamAPlayerIds: string[];
  teamBPlayerIds: string[];
  teamAScore: number;
  teamBScore: number;
  winner: 'A' | 'B';
  jirkaLunakId?: string;
}

// Background Images API
export interface BackgroundImagesResponse {
  images: string[];
  rotationInterval: number; // seconds
}

// App Configuration API
export interface AppConfigurationResponse {
  appName: string;
  isSeasoningSupported: boolean;
  nonRegularsTitle: string;
  playerLimit: number;
  overLimitMessage: string;
  isSmallMatchesEnabled: boolean;
  isJirkaLunakEnabled: boolean;
  isDoodleEnabled: boolean;
}
