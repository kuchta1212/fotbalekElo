/**
 * API service methods for all endpoints
 */

import { get, post, put, buildQueryString, type FetchOptions } from './api';
import type {
  LeaderboardResponse,
  PlayersListResponse,
  PlayerDetailResponse,
  MatchesListResponse,
  MatchDetailResponse,
  MatchFilters,
  DoodleUpcomingResponse,
  DoodleDetailResponse,
  UpdateAvailabilityRequest,
  UpdateAvailabilityResponse,
  GenerateTeamsRequest,
  GenerateTeamsResponse,
  AddPlayerRequest,
  AddMatchRequest,
  BackgroundImagesResponse,
  AppConfigurationResponse,
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
  list: (filters?: MatchFilters) => {
    const query = filters ? buildQueryString(filters as Record<string, unknown>) : '';
    return get<MatchesListResponse>(`/api/matches${query}`);
  },
  get: (id: string) => get<MatchDetailResponse>(`/api/matches/${id}`),
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
  generate: (request: GenerateTeamsRequest) =>
    post<GenerateTeamsResponse>('/api/teams/generate', request),
};

export const configService = {
  get: () => get<AppConfigurationResponse>('/api/config'),
  getBackgroundImages: () => get<BackgroundImagesResponse>('/api/background-images'),
};

// Admin endpoints (require Basic Auth)

export const adminService = {
  addPlayer: (request: AddPlayerRequest, auth: { username: string; password: string }) =>
    post<{ id: string }>('/api/admin/players', request, {
      useBasicAuth: true,
      ...auth,
    } as FetchOptions),
  
  addMatch: (request: AddMatchRequest, auth: { username: string; password: string }) =>
    post<{ id: string }>('/api/admin/matches', request, {
      useBasicAuth: true,
      ...auth,
    } as FetchOptions),
};
