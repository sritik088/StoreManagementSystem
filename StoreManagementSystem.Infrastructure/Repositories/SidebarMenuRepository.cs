using Microsoft.EntityFrameworkCore;
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Infrastructure.Data;

namespace StoreManagementSystem.Infrastructure.Repositories
{
    public class SidebarMenuRepository : ISidebarMenuRepository
    {
        private readonly ApplicationDbContext _context;

        public SidebarMenuRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET SIDEBAR
        // =====================================================

        public async Task<List<SidebarMenuSection>>
            GetSectionsAsync()
        {
            return await _context.SidebarMenuSections
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }


        // =====================================================
        // CHECK EXISTING MENU
        // =====================================================

        public Task<bool> AnyAsync()
        {
            return _context.SidebarMenuSections
                .AnyAsync();
        }


        // =====================================================
        // SAVE ARRANGEMENT
        // =====================================================
        //
        // Supports:
        //
        // 1. Changing section position
        // 2. Changing item position
        // 3. Moving item between sections
        //
        // Example:
        //
        // Units
        // MASTER DATA -> INVENTORY
        //
        // Purchase Orders
        // PURCHASE -> MASTER DATA
        //
        // =====================================================

        public async Task SaveArrangementAsync(
            List<SidebarMenuSection> sections)
        {
            if (sections == null ||
                sections.Count == 0)
            {
                return;
            }


            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =================================================
                // LOAD ALL SECTIONS + ALL ITEMS
                // =================================================

                var dbSections =
                    await _context.SidebarMenuSections
                        .Include(x => x.Items)
                        .ToListAsync();


                // =================================================
                // SECTION LOOKUP
                // =================================================

                var sectionMap =
                    dbSections.ToDictionary(
                        x => x.Id);


                // =================================================
                // ITEM LOOKUP
                //
                // IMPORTANT:
                // Items are collected from ALL sections.
                //
                // This is what allows an item to move from
                // one section to another.
                // =================================================

                var itemMap =
                    dbSections
                        .SelectMany(x => x.Items)
                        .ToDictionary(
                            x => x.Id);


                var submittedSectionIds =
                    new HashSet<int>();


                var submittedItemIds =
                    new HashSet<int>();


                // =================================================
                // 1. UPDATE SECTION POSITIONS
                // =================================================

                foreach (var incomingSection in sections)
                {
                    if (!sectionMap.TryGetValue(
                            incomingSection.Id,
                            out var existingSection))
                    {
                        continue;
                    }


                    // Prevent duplicate section submissions.
                    if (!submittedSectionIds.Add(
                            existingSection.Id))
                    {
                        continue;
                    }


                    // Use the position entered by the user.
                    //
                    // Example:
                    //
                    // DASHBOARD       = 1
                    // MASTER DATA     = 2
                    // PURCHASE        = 3
                    // INVENTORY       = 4

                    existingSection.DisplayOrder =
                        incomingSection.DisplayOrder > 0
                            ? incomingSection.DisplayOrder
                            : existingSection.DisplayOrder;
                }


                // =================================================
                // 2. UPDATE ITEM SECTION + POSITION
                // =================================================
                //
                // The incoming item collection may be in any order.
                //
                // We therefore:
                //
                // 1. Convert ICollection -> List
                // 2. Sort by entered DisplayOrder
                // 3. Move item to selected section
                // 4. Assign normalized positions
                //
                // =================================================

                foreach (var incomingSection in sections)
                {
                    if (!sectionMap.ContainsKey(
                            incomingSection.Id))
                    {
                        continue;
                    }


                    // IMPORTANT:
                    //
                    // ICollection<T> does not support:
                    //
                    // incomingSection.Items[index]
                    //
                    // Convert it to List first.

                    var incomingItems =
                        incomingSection.Items?
                            .ToList()
                        ?? new List<SidebarMenuItem>();


                    // =================================================
                    // SORT ITEMS BY USER ENTERED POSITION
                    // =================================================

                    var orderedItems =
                        incomingItems
                            .OrderBy(x => x.DisplayOrder)
                            .ThenBy(x => x.Id)
                            .ToList();


                    // =================================================
                    // NORMALIZED POSITION
                    // =================================================

                    int itemPosition = 1;


                    foreach (var incomingItem in orderedItems)
                    {
                        // =================================================
                        // FIND ITEM FROM DATABASE
                        // =================================================

                        if (!itemMap.TryGetValue(
                                incomingItem.Id,
                                out var existingItem))
                        {
                            continue;
                        }


                        // Prevent duplicate submission.
                        if (!submittedItemIds.Add(
                                existingItem.Id))
                        {
                            continue;
                        }


                        // =================================================
                        // MOVE ITEM TO SELECTED SECTION
                        // =================================================

                        existingItem.SectionId =
                            incomingSection.Id;


                        // =================================================
                        // SAVE ITEM POSITION
                        // =================================================
                        //
                        // Example user enters:
                        //
                        // Products        = 4
                        // Suppliers       = 2
                        // Units           = 1
                        //
                        // Final order:
                        //
                        // Units           = 1
                        // Suppliers      = 2
                        // Products        = 3
                        //
                        // =================================================

                        existingItem.DisplayOrder =
                            itemPosition++;

                    }
                }


                // =================================================
                // 3. NORMALIZE SECTION POSITIONS
                // =================================================
                //
                // If the user enters:
                //
                // Dashboard       = 1
                // Inventory       = 5
                // Purchase        = 2
                //
                // The resulting order becomes:
                //
                // Dashboard       = 1
                // Purchase        = 2
                // Inventory       = 3
                //
                // This prevents duplicate/gapped positions.
                //
                // =================================================

                var orderedSections =
                    dbSections
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Id)
                        .ToList();


                int sectionPosition = 1;


                foreach (var section in orderedSections)
                {
                    section.DisplayOrder =
                        sectionPosition++;
                }


                // =================================================
                // 4. SAVE CHANGES
                // =================================================

                await _context.SaveChangesAsync();


                // =================================================
                // 5. COMMIT TRANSACTION
                // =================================================

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }


        // =====================================================
        // ADD DEFAULT MENU
        // =====================================================

        public async Task AddSectionsAsync(
            List<SidebarMenuSection> sections)
        {
            if (sections == null ||
                sections.Count == 0)
            {
                return;
            }


            await _context.SidebarMenuSections
                .AddRangeAsync(sections);


            await _context.SaveChangesAsync();
        }
    }
}