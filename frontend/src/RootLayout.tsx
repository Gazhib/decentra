import { Outlet } from "react-router";
import Header from "./widget/Header";
import Footer from "./widget/Footer";
import { useUser } from "./entities/user/model/useUser";

export default function RootLayout() {
  useUser();
  return (
    <>
      <Header />
      <Outlet />
      <Footer />
    </>
  );
}
