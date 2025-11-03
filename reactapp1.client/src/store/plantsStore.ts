import { create } from 'zustand';
import { IPlant } from '../interfaces/IPlant';
import { IPest } from '../interfaces/IPest';
import { ISoil } from '../interfaces/ISoil';
import { getApi, postApi, putApi, deleteApi } from '../api';

type PlantsState = {
  // data
  plants: IPlant[];
  allPests: IPest[];
  allSoils: ISoil[];

  // ui
  visibleModal: boolean;
  editPlant?: IPlant;

  // filters
  filterPest: string;
  filterSoil: string;
  query: string;

  // actions
  loadPlants: () => Promise<void>;
  ensureCatalogs: () => Promise<void>;
  openModal: (p?: IPlant) => void;
  closeModal: () => void;
  setFilterPest: (v: string) => void;
  setFilterSoil: (v: string) => void;
  setQuery: (v: string) => void;
  clearFilters: () => void;
  savePlant: (plant: IPlant) => Promise<void>;
  removePlant: (id?: number) => Promise<void>;
};

export const usePlantsStore = create<PlantsState>((set, get) => ({
  plants: [],
  allPests: [],
  allSoils: [],

  visibleModal: false,
  editPlant: undefined,

  filterPest: '',
  filterSoil: '',
  query: '',

  loadPlants: async () => {
    const data = await getApi<IPlant[]>('plants');
    if (data) set({ plants: data });
  },

  ensureCatalogs: async () => {
    const { allPests, allSoils } = get();
    if (allPests.length === 0) {
      const pests = await getApi<IPest[]>('pests');
      if (pests) set({ allPests: pests });
    }
    if (allSoils.length === 0) {
      const soils = await getApi<ISoil[]>('soils');
      if (soils) set({ allSoils: soils });
    }
  },

  openModal: (p) => set({ visibleModal: true, editPlant: p }),
  closeModal: () => set({ visibleModal: false, editPlant: undefined }),

  setFilterPest: (v) => set({ filterPest: v }),
  setFilterSoil: (v) => set({ filterSoil: v }),
  setQuery: (v) => set({ query: v }),
  clearFilters: () => set({ filterPest: '', filterSoil: '', query: '' }),

  savePlant: async (plant: IPlant) => {
    if (plant.id) {
      await putApi(`plants/${plant.id}`, plant);
    } else {
      await postApi('plants', plant);
    }
    await get().loadPlants();
    set({ visibleModal: false, editPlant: undefined });
  },

  removePlant: async (id?: number) => {
    if (!id) return;
    await deleteApi(`plants/${id}`, {});
    await get().loadPlants();
    set({ visibleModal: false, editPlant: undefined });
  },
}));