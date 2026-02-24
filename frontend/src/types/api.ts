/**
 * API Request/Response DTOs
 */

import type { Season, Player, Match, GeneratorResult, LeaderboardEntry, PlayerStats } from './domain';

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
export interface DoodlePlayerDto {
  name: string;
  status: 'Accept' | 'Maybe' | 'Refused' | 'NoAnswer';
}

export interface DoodleDateDto {
  date: string; // yyyy-MM-dd
  displayDate: string; // dd.MM
  players: DoodlePlayerDto[];
}

export interface DoodleStatsDto {
  coming: number;
  maybe: number;
  refused: number;
}

export interface DoodleUpcomingResponse {
  dates: DoodleDateDto[];
  stats: DoodleStatsDto;
  playerLimit: number;
  overLimitMessage: string;
  isSeasoningSupported: boolean;
}

export interface DoodleDetailResponse {
  date: string;
  displayDate: string;
  players: DoodlePlayerDto[];
}

export interface UpdateAvailabilityRequest {
  playerName: string;
  status: 'Accept' | 'Maybe' | 'Refused' | 'NoAnswer';
}

export interface UpdateAvailabilityResponse {
  success: boolean;
  stats: DoodleStatsDto;
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
