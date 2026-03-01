/**
 * API service methods for all endpoints
 */

import { get, post, put, buildQueryString, type FetchOptions } from './api';
import type {
  LeaderboardResponse,
  PlayerDetailResponse,
  PlayersListResponse,
  DoodleUpcomingResponse,
  DoodleDetailResponse,
  UpdateAvailabilityRequest,
  UpdateAvailabilityResponse,
  BackgroundImagesResponse,
  AppConfigurationResponse,
  GenerateTeamsResponse,
  AddPlayerRequest,
  AddMatchRequest,
  AddMatchResponse,
  MatchPlayersResponse,
} from '@/types/api';
import type { Season } from '@/types/domain';

// Public endpoints

export const leaderboardService = {
  get: (season: Season = 'Overall') => 
    get<LeaderboardResponse>(`/api/leaderboards${buildQueryString({ season })}`),
};

export const playersService = {
  list: () => get<PlayersListResponse>('/api/players'),
  get: (id: string) => get<PlayerDetailResponse>(`/api/players/${id}`),
};

export const matchesService = {
  getPlayersForMatch: () => get<MatchPlayersResponse>('/api/matches/players'),
};

export const doodleService = {
  upcoming: (count: number = 5) => 
    get<DoodleUpcomingResponse>(`/api/doodle/upcoming${buildQueryString({ count })}`),
  get: (date: string) => get<DoodleDetailResponse>(`/api/doodle/${date}`),
  updateAvailability: (date: string, request: UpdateAvailabilityRequest) =>
    put<UpdateAvailabilityResponse>(`/api/doodle/${date}/availability`, request),
  advancePoll: (auth: { username: string; password: string }) =>
    post<{ success: boolean; message: string; removedDate: string; addedDate: string }>(
      '/api/doodle/advance-poll',
      {},
      {
        useBasicAuth: true,
        ...auth,
      } as FetchOptions
    ),
};

export const teamsService = {
  generate: (date: string, season: string) =>
    get<GenerateTeamsResponse>(`/api/teams/generate${buildQueryString({ date, season })}`),
};

export const configService = {
  get: () => get<AppConfigurationResponse>('/api/config'),
  getBackgroundImages: () => get<BackgroundImagesResponse>('/api/background-images'),
};

// Admin endpoints (require Basic Auth)

export const adminService = {
addPlayer: (request: AddPlayerRequest, auth: { username: string; password: string }) =>
  post<{ id: string; name: string; message: string }>('/api/players', request, {
    useBasicAuth: true,
    ...auth,
  } as FetchOptions),
  
  addMatch: (request: AddMatchRequest, auth: { username: string; password: string }) =>
    post<AddMatchResponse>('/api/matches', request, {
      useBasicAuth: true,
      ...auth,
    } as FetchOptions),
};
