import { create } from "zustand";

interface User {
  id: string;
  role: string;
  name: string;
  surname: string;
  phoneNumber: string;
  lastDate: string;
}

interface UserStore {
  user: User | null;
  setUser: (user: User) => void;
}

export const userStore = create<UserStore>((set) => ({
  user: null,
  setUser: (user: User) => set({ user }),
}));
