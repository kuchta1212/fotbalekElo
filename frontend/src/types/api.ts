/**
 * API Request/Response DTOs
 */

import type { Season, Player, Match, LeaderboardEntry, PlayerStats } from './domain';

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

// Teams Generator API
export interface TeamPlayerDto {
  id: string;
  name: string;
  elo: number;
  overallElo: number;
}

export interface TeamDto {
  teamElo: number;
  players: TeamPlayerDto[];
}

export interface GeneratorResultDto {
  teamOne: TeamDto;
  teamTwo: TeamDto;
  eloDiff: number;
  season: string;
}

export interface GenerateTeamsRequestBody {
  playerIds: string[];
  substituteIds?: string[];
  season: string;
}

export interface GenerateTeamsResponse {
  results: GeneratorResultDto[];
  count: number;
  season: string;
}

export interface DoodlePlayerOptionDto {
  id: string;
  name: string;
  elo: number;
  summerElo: number;
  winterElo: number;
}

export interface PlayersFromDoodleResponse {
  date: string;
  displayDate: string;
  players: DoodlePlayerOptionDto[];
  count: number;
}

// Admin API
export interface AddPlayerRequest {
  name: string;
  initialElo?: number;
}

export interface AddMatchRequest {
  winnerPlayerIds: string[];
  loserPlayerIds: string[];
  winnerScore: number;
  loserScore: number;
  weight: 'BigMatch' | 'SmallMatch';
  season: string;
  heroId?: string;
}

export interface AddMatchResponse {
  message: string;
  score: string;
  winnerEloChange: number;
  loserEloChange: number;
}

export interface MatchPlayerOptionDto {
  id: string;
  name: string;
}

export interface MatchPlayersResponse {
  players: MatchPlayerOptionDto[];
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
