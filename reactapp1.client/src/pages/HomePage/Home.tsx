import { Link } from "react-router-dom";

export default function Home() {
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
                </div>
            </section>

            
        </div>
    );
}