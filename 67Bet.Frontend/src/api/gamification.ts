import api from "./axios";

export type UserProgress = {
  userId: string;
  experiencePoints: number;
  currentLevel: number;
  nextLevelXp: number;
  progressPercentage: number;
};

export type Achievement = {
  achievementId: string;
  name: string;
  description: string;
  currentProgress: number;
  threshold: number;
  isUnlocked: boolean;
  unlockedAt?: string;
  iconUrl: string;
  type: string;
};

export const getMyProgress = async (): Promise<UserProgress> => {
  const response = await api.betting.get<UserProgress>(
    "/gamification/me/progress",
  );
  return response.data;
};

export const getMyAchievements = async (): Promise<Achievement[]> => {
  const response = await api.betting.get<Achievement[]>(
    "/gamification/me/achievements",
  );
  return response.data;
};

export const processDailyLogin = async (): Promise<void> => {
  await api.betting.post("/gamification/me/daily-login");
};
