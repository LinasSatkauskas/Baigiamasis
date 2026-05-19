import { Link } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";

export default function Home() {
    const isAdmin = useAuthStore(s => s.isAdmin);

    return (
        <div className="space-y-6">
            <section className="rounded-xl bg-gradient-to-r from-emerald-600 to-teal-500 text-white p-8">
                <h1 className="text-3xl font-bold mb-2">Sveiki atvykę</h1>
                <p className="text-white/90 mb-6 max-w-prose">
                    Tvarkykite augalus, dirvožemius ir kenkėjus vienoje vietoje.
                </p>
                <div className="flex gap-3 flex-wrap">
                    <Link
                        to="/plants"
                        className="bg-white text-emerald-700 font-medium px-5 py-2.5 rounded-lg hover:bg-emerald-50"
                    >
                        Eiti į augalus
                    </Link>
                    {isAdmin() && (
                        <>
                            <Link
                                to="/pests"
                                className="bg-white/10 backdrop-blur px-5 py-2.5 rounded-lg hover:bg-white/20"
                            >
                                Kenkėjai
                            </Link>
                            <Link
                                to="/soils"
                                className="bg-white/10 backdrop-blur px-5 py-2.5 rounded-lg hover:bg-white/20"
                            >
                                Dirvožemiai
                            </Link>
                        </>
                    )}
                </div>
            </section>

            {/* Stylized description */}
            <section className="rounded-xl border border-emerald-200 bg-white shadow-sm p-6">
                <h2 className="text-xl font-semibold text-emerald-900 mb-3">
                    Ką rasite šiame puslapyje
                </h2>
                <ul className="grid gap-3 sm:grid-cols-2 text-gray-700">
                    <li className="flex items-start gap-3">
                        <span className="text-emerald-600 text-xl mt-0.5">🌿</span>
                        <div>
                            <div className="font-medium">Augalų katalogas</div>
                            <div className="text-sm text-gray-600">
                                Peržiūrėkite augalų nuotraukas ir aprašymus vienoje vietoje.
                            </div>
                        </div>
                    </li>
                    <li className="flex items-start gap-3">
                        <span className="text-emerald-600 text-xl mt-0.5">🪲</span>
                        <div>
                            <div className="font-medium">Išmanūs filtrai</div>
                            <div className="text-sm text-gray-600">
                                Filtruokite pagal kenkėjus ir dirvožemio tipą, naudokite paiešką.
                            </div>
                        </div>
                    </li>
                    <li className="flex items-start gap-3">
                        <span className="text-emerald-600 text-xl mt-0.5">💬</span>
                        <div>
                            <div className="font-medium">Komentarai</div>
                            <div className="text-sm text-gray-600">
                                Prisijungę vartotojai gali rašyti ir redaguoti savo komentarus.
                            </div>
                        </div>
                    </li>
                    <li className="flex items-start gap-3">
                        <span className="text-emerald-600 text-xl mt-0.5">🛡️</span>
                        <div>
                            <div className="font-medium">Teisės ir sauga</div>
                            <div className="text-sm text-gray-600">
                                {isAdmin()
                                    ? "Galite pridėti ir redaguoti augalus, kenkėjus bei dirvožemius, taip pat tvarkyti komentarus (įskaitant kelių komentarų ištrynimą vienu metu)."
                                    : "Administratoriai gali pridėti ir redaguoti augalus, kenkėjus bei dirvožemius, taip pat tvarkyti komentarus."}
                            </div>
                        </div>
                    </li>
                    <li className="flex items-start gap-3">
                        <span className="text-emerald-600 text-xl mt-0.5">⚙️</span>
                        <div>
                            <div className="font-medium">Patogi sąsaja</div>
                            <div className="text-sm text-gray-600">
                                Reaguojantis dizainas – patogu naudotis telefone ir kompiuteryje.
                            </div>
                        </div>
                    </li>
                </ul>
            </section>
        </div>
    );
}