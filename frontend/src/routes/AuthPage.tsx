import {
  redirect,
  useNavigate,
  useSearchParams,
} from "react-router";
import Auth from "../features/authentication/Auth";
import { useEffect } from "react";
import { motion } from "framer-motion";
export default function AuthPage() {
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode");
  const isLogin = mode === "login";
  const isRegistration = mode === "registration";
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLogin && !isRegistration) navigate("/auth?mode=login");
  }, [isLogin, isRegistration, navigate]);

  return (
    <main className="w-full h-full flex lg:flex-row items-center flex-col">
      <Auth mode={mode || ""} />
      <section className="flex-5 bg-[#a7e92f] h-full lg:flex items-center justify-center hidden ">
        <motion.span
          initial={{ opacity: 0, y: 50 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="text-black text-[7rem]"
        >
          inDrive
        </motion.span>
      </section>
    </main>
  );
}

export async function action({ request }: { request: Request }) {
  const fd = await request.formData();
  const url = new URL(request.url);
  const mode = url.searchParams.get("mode");
  const fetchUrl = `/api/auth/${mode === "login" ? "Login" : "CreateUser"}`;

  const phone = fd.get("phone");
  const name = fd.get("name");
  const surname = fd.get("surname");
  const password = fd.get("password");
  const confirmPassword = fd.get("confirmpassword");

  if (mode === "login" || mode === "registration") {
    const response = await fetch(fetchUrl, {
      method: "POST",
      credentials: "include",
      body: JSON.stringify({ phone, password, name, surname, role: "user" }),
      headers: {
        "Content-Type": "application/json",
      },
    });

    const responseData = await response.json();

    if (response.ok) {
      return redirect("/car-verification");
    } else {
      return {
        message:
          responseData ||
          `${mode === "login" ? "Login" : "Registration"} failed`,
      };
    }
  } else {
    return redirect("/auth?mode=login");
  }
}
