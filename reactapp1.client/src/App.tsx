import React, { PropsWithChildren } from "react"
import { createBrowserRouter, RouterProvider, Navigate } from "react-router-dom"
import Home from "./pages/HomePage/Home"
import { Layout } from "./pages/Layout"
import Plants from "./pages/PlantsPage/Plants"
import Pests from "./pages/PestsPage/Pests"
import Soils from "./pages/SoilsPage/Soils"
import { useAuthStore } from "@/store/authStore"
import ResetPasswordPage from "./pages/Auth/ResetPasswordPage"

function RequireAdmin({ children }: PropsWithChildren) {
  const isAdmin = useAuthStore((s) => s.isAdmin)
  const user = useAuthStore((s) => s.user)

  // If not logged in or not admin, redirect to home.
  if (!user || !isAdmin()) return <Navigate to="/" replace />
  return <>{children}</>
}

export default function App() {
  const router = createBrowserRouter([
    {
      path: "/reset-password",
      Component: ResetPasswordPage,
    },
    {
      path: "/",
      Component: Layout,
      children: [
        { index: true, Component: Home },
        { path: "plants", Component: Plants },
        {
          path: "pests",
          element: (
            <RequireAdmin>
              <Pests />
            </RequireAdmin>
          ),
        },
        {
          path: "soils",
          element: (
            <RequireAdmin>
              <Soils />
            </RequireAdmin>
          ),
        },
      ],
    },
  ])

  return <RouterProvider router={router} />
}
