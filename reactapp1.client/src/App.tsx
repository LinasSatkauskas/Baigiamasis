import {
    createBrowserRouter,
    RouterProvider,
}
    from "react-router-dom";
import Home from "./pages/HomePage/Home";
import { Layout } from "./pages/Layout";
import Plants from "./pages/PlantsPage/Plants";
import Pests from "./pages/PestsPage/Pests"; // added
import Soils from "./pages/SoilsPage/Soils";

export default function App() {
    const router = createBrowserRouter([
        {
            path: "/",
            Component: Layout,
            children: [
                { index: true, Component: Home },
                { path: "plants", Component: Plants },
                { path: "pests", Component: Pests },
                { path: "soils", Component: Soils }
            ]
        },
    ]);

    return <RouterProvider router={router} />;
}