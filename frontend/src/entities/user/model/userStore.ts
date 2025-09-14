import { create } from "zustand";

interface User {
  userId: string;
  role: string;
  name: string;
  surname: string;
  phone: string;
  lastDate: string;
  appealId: number;
}

interface UserStore {
  user: User | null;
  setUser: (user: User | null) => void;
}

export const userStore = create<UserStore>((set) => ({
  user: null,
  setUser: (user: User | null) => set({ user }),
}));
